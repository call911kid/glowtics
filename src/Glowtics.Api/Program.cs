
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Services;
using Glowtics.BLL.Settings;
using Glowtics.Api.Middleware;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            builder.Services.AddIdentityCore<GlowticsUser>()
           .AddRoles<IdentityRole<Guid>>()
           .AddEntityFrameworkStores<GlowticsDbContext>();

            // Register BLL Services
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IBllAssemblyMarker).Assembly));
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

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
            });

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
