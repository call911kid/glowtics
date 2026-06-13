
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Glowtics.BLL.Commands.LoginCommand).Assembly));
            // Register AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
