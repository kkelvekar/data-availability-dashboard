using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DaDashboard.Persistence.Configuration
{
    public class BusinessEntityConfigConfiguration : IEntityTypeConfiguration<BusinessEntityConfig>
    {
        public void Configure(EntityTypeBuilder<BusinessEntityConfig> builder)
        {
            builder.ToTable("BusinessEntityConfig");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)");  // JSON formatted metadata

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            builder.Property(e => e.UpdatedDate)
                .IsRequired();
        }
    }
}
