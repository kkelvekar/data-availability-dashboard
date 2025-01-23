using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Persistence.Configuration
{
    public class DataDomainConfigConfiguration : IEntityTypeConfiguration<DataDomainConfig>
    {
        public void Configure(EntityTypeBuilder<DataDomainConfig> builder)
        {
            builder.ToTable("DataDomainConfig");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.DomainName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.SourceType)
                .HasMaxLength(50);

            builder.Property(e => e.IsActive)
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            builder.Property(e => e.UpdatedDate)
                .IsRequired();

            // One-to-One Relationship
            builder.HasOne(e => e.DomainSourceGraphQL)
                .WithOne(e => e.DataDomainConfig)
                .HasForeignKey<DomainSourceTypeGraphQL>(e => e.DataDomainId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
