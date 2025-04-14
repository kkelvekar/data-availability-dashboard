using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DaDashboard.Persistence
{
    public class DaDashboardDbContext : DbContext
    {
        public DaDashboardDbContext(DbContextOptions<DaDashboardDbContext> options) : base(options)
        {
        }

        public DbSet<BusinessEntityConfig> BusinessEntityConfigs { get; set; }
        public DbSet<BusinessEntityRAGConfig> BusinessEntityRAGConfigs { get; set; }
        public DbSet<BusinessEntity> BusinessEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DaDashboardDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTime.Now;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
