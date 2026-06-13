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
    public class RetailerConfiguration : IEntityTypeConfiguration<Retailer>
    {
        public void Configure(EntityTypeBuilder<Retailer> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Domain)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(r => r.Domain) //avoids full table scan
                .IsUnique();

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.ApiKeyHash)
                .HasMaxLength(500);

            builder.HasIndex(r => r.ApiKeyHash)
                .IsUnique()
                .HasFilter("[ApiKeyHash] IS NOT NULL"); // avoids unique constraint violation for multiple null values

            builder.Property(r => r.ApiKeyHint)
                .HasMaxLength(16);

            builder.Property(r => r.ProductEndpoint)
                .HasMaxLength(2048);

            builder.Property(r => r.CartRedirectUrl)
                .HasMaxLength(2048);

            builder.Property(r => r.MongoCollectionName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            builder.Property(r => r.IsDeleted)
                .IsRequired();

            builder.HasQueryFilter(r => !r.IsDeleted);

            builder.HasOne(r => r.User)
                .WithOne()
                .HasForeignKey<Retailer>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
