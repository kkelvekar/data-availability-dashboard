using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DaDashboard.Persistence.Configuration
{
    public class BusinessEntityConfiguration : IEntityTypeConfiguration<BusinessEntity>
    {
        public void Configure(EntityTypeBuilder<BusinessEntity> builder)
        {
            builder.ToTable("BusinessEntity");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.ApplicationOwner)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.DependentFunctionalities)
                .HasColumnType("nvarchar(max)");  // Can be used to store JSON or comma-separated values

            builder.Property(e => e.IsActive)
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            builder.Property(e => e.UpdatedDate)
                .IsRequired();

            // Relationships
            // Relationship with BusinessEntityConfig
            builder.HasOne(e => e.BusinessEntityConfig)
                .WithMany()   // If BusinessEntityConfig will have a collection, use .WithMany(c => c.BusinessEntities)
                .HasForeignKey(e => e.BusinessEntityConfigId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with BusinessEntityRAGConfig
            builder.HasOne(e => e.BusinessEntityRAGConfig)
                .WithMany()   // If BusinessEntityRAGConfig will have a collection, use .WithMany(r => r.BusinessEntities)
                .HasForeignKey(e => e.BusinessEntityRAGConfigId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
