﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Persistence.Configuration
{
    public class DomainSourceTypeGraphQLConfiguration : IEntityTypeConfiguration<DomainSourceTypeGraphQL>
    {
        public void Configure(EntityTypeBuilder<DomainSourceTypeGraphQL> builder)
        {
            builder.ToTable("DomainSourceGraphQL");

            // Primary Key
            builder.HasKey(e => e.DataDomainId);

            // Properties
            builder.Property(e => e.DevBaseUrl)
                .HasMaxLength(2000);

            builder.Property(e => e.QaBaseUrl)
                .HasMaxLength(2000);

            builder.Property(e => e.PreProdBaseUrl)
                .HasMaxLength(2000);

            builder.Property(e => e.ProdBaseUrl)
                .HasMaxLength(2000);

            builder.Property(e => e.EndpointPath)
                .HasMaxLength(500);

            builder.Property(e => e.EntityKey)
                .HasMaxLength(100);

            // Navigation
            builder.HasOne(e => e.DataDomainConfig)
                .WithOne(e => e.DomainSourceGraphQL)
                .HasForeignKey<DomainSourceTypeGraphQL>(e => e.DataDomainId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
