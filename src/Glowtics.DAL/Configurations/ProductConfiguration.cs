using Glowtics.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glowtics.DAL.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            // Global Query Filter to automatically exclude soft-deleted products
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.Property(p => p.ExternalProductId)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasOne(p => p.Retailer)
                .WithMany(r => r.Products)
                .HasForeignKey(p => p.RetailerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
