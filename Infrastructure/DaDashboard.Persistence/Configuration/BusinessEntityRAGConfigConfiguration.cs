using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DaDashboard.Persistence.Configuration
{
    public class BusinessEntityRAGConfigConfiguration : IEntityTypeConfiguration<BusinessEntityRAGConfig>
    {
        public void Configure(EntityTypeBuilder<BusinessEntityRAGConfig> builder)
        {
            builder.ToTable("BusinessEntityRAGConfig");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.RedExpression)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.AmberExpression)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.GreenExpression)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            builder.Property(e => e.UpdatedDate)
                .IsRequired();
        }
    }
}
