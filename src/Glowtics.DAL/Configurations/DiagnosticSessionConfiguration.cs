using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glowtics.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glowtics.DAL.Configurations
{
    public class DiagnosticSessionConfiguration : IEntityTypeConfiguration<DiagnosticSession>
    {
        public void Configure(EntityTypeBuilder<DiagnosticSession> builder)
        {
            builder.HasKey(ds => ds.Id);

            builder.Property(ds => ds.SkinProfileResult)
                .IsRequired();

            builder.HasMany(ds => ds.RecommendedProducts)
                .WithMany(p => p.DiagnosticSessions);

            builder.Property(ds => ds.CreatedAt)
                .IsRequired();

            builder.Property(ds => ds.ImageHash)
                .HasMaxLength(64);

            // Dedup lookup: same retailer + same user + same image -> return the old session.
            builder.HasIndex(ds => new { ds.RetailerId, ds.ExternalUserId, ds.ImageHash });

            builder.HasOne(ds => ds.Retailer)
                .WithMany(r => r.DiagnosticSessions)
                .HasForeignKey(ds => ds.RetailerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
