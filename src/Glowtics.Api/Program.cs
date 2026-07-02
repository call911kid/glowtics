using FluentValidation;
using FluentValidation.AspNetCore;
using Glowtics.BLL;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Services;
using Glowtics.BLL.Settings;
using Glowtics.Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;
using Glowtics.Api.Authentication;
using Glowtics.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Glowtics.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration)
                             .WriteTo.Console()
                             .WriteTo.File("logs/glowtics-.txt", rollingInterval: RollingInterval.Day)
            );

            // Add services to the container.

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState.Keys
                            .SelectMany(key => context.ModelState[key]!.Errors.Select(error => $"{key}: {error.ErrorMessage}"))
                            .ToList();

                        var response = ApiResponse.Failure("VALIDATION_ERROR", "One or more fields failed validation.", errors);
                        return new BadRequestObjectResult(response);
                    };
                });
            builder.Services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters()
                            .AddValidatorsFromAssembly(typeof(Program).Assembly);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Unable to find the default connection string.");

            builder.Services.AddDbContext<GlowticsDbContext>(options =>
                options.UseSqlServer(connectionString));

            var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection") 
                ?? throw new InvalidOperationException("Unable to find the MongoDB connection string.");
            var mongoDatabaseName = builder.Configuration["MongoDatabaseName"] 
                ?? throw new InvalidOperationException("Unable to find the MongoDB database name.");

            builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
            builder.Services.AddScoped<IMongoDatabase>(sp => 
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDatabaseName);
            });

            builder.Services.AddIdentityCore<GlowticsUser>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
           .AddRoles<IdentityRole<Guid>>()
           .AddEntityFrameworkStores<GlowticsDbContext>()
           .AddDefaultTokenProviders();

            // Register BLL Services
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IBllAssemblyMarker).Assembly));
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();
            builder.Services.AddTransient<IEmailService, SmtpEmailService>();
            builder.Services.AddTransient<IEmailNotificationService, EmailNotificationService>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(ApiKeySettings.SectionName));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<AdvancedLangflowSettings>(builder.Configuration.GetSection(AdvancedLangflowSettings.SectionName));
            builder.Services.Configure<LangfuseSettings>(builder.Configuration.GetSection(LangfuseSettings.SectionName));
            builder.Services.Configure<CohereSettings>(builder.Configuration.GetSection(CohereSettings.SectionName));
            builder.Services.AddHttpClient<IAnalysisTracer, AnalysisTracer>();

            // Backend-side product embedding (Cohere) — replaces the HF-MiniLM Langflow embedding workflow.
            builder.Services.AddHttpClient<IEmbeddingService, CohereEmbeddingService>(c => c.Timeout = TimeSpan.FromSeconds(30));

            builder.Services.AddHttpClient<IAdvancedLangflowService, AdvancedLangflowService>((provider, client) =>
            {
                var settings = provider.GetRequiredService<IOptions<AdvancedLangflowSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
                client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");

                // ngrok tunnel is protected by HTTP Basic auth; send it when configured (else 401 from ngrok).
                if (!string.IsNullOrEmpty(settings.BasicAuthUsername))
                {
                    var basic = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes($"{settings.BasicAuthUsername}:{settings.BasicAuthPassword}"));
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basic);
                }
            });

            // Register AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(
                typeof(Program).Assembly, 
                typeof(IBllAssemblyMarker).Assembly
            ));
            // Configure Authentication
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key ?? string.Empty))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(ApiResponse.Failure("ERR_UNAUTHORIZED", "Authentication failed. Token is missing, invalid, or expired.", new List<string>()));
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(ApiResponse.Failure("ERR_FORBIDDEN", "You do not have the required permissions or role to access this resource.", new List<string>()));
                        return context.Response.WriteAsync(result);
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
