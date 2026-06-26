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

namespace Glowtics.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
            builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection(LangflowSettings.SectionName));

            builder.Services.AddHttpClient<ILangflowService, LangflowService>((provider, client) => 
            {
                var settings = provider.GetRequiredService<IOptions<LangflowSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.ApiKey}");
                client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
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
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
