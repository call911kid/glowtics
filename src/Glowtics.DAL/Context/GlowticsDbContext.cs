using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.DAL.Context
{
    public class GlowticsDbContext:IdentityDbContext<GlowticsUser, IdentityRole<Guid>, Guid>
    {
        public GlowticsDbContext(DbContextOptions<GlowticsDbContext> options) : base(options) { }

        public DbSet<Retailer> Retailers { get; set; }
        public DbSet<DiagnosticSession> DiagnosticSessions { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(GlowticsDbContext).Assembly);
        }

    }
}
