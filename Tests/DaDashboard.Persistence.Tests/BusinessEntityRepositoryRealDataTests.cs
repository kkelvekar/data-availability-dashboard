using System;
using System.Linq;
using System.Threading.Tasks;
using DaDashboard.Domain.Entities;
using DaDashboard.Persistence;
using DaDashboard.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaDashboard.Persistence.Tests.Repositories
{
    [TestClass]
    public class BusinessEntityRepositoryRealDataTests
    {
        // Adjust the connection string as needed to target the actual SQL Server database.
        private DbContextOptions<DaDashboardDbContext> CreateSqlServerContextOptions()
        {
            return new DbContextOptionsBuilder<DaDashboardDbContext>()
                .UseSqlServer("Server=KAUSTUBH-PC;Database=da-dashboard-db;Integrated Security=True;TrustServerCertificate=True;")
                .Options;
        }

        /// <summary>
        /// A simple test to verify if the GetActiveBusinessEntitiesWithDetailsAsync method
        /// returns some active business entity data from the actual database.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetActiveBusinessEntitiesWithDetailsAsync_ReturnsData()
        {
            var options = CreateSqlServerContextOptions();

            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                var activeEntities = await repository.GetActiveBusinessEntitiesWithDetailsAsync();

                Assert.IsNotNull(activeEntities, "The repository method should not return null.");
                // Ensure that there is at least one active entity.
                Assert.IsTrue(activeEntities.Any(), "Expected at least one active business entity in the database.");

                // Optionally, output details for debugging.
                foreach (var entity in activeEntities)
                {
                    Console.WriteLine($"BusinessEntity ID: {entity.Id}, Name: {entity.Name}");
                }
            }
        }

        /// <summary>
        /// A simple test to verify if the GetBusinessEntityNameByIdAsync method can retrieve
        /// a name from the actual data in the database.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBusinessEntityNameByIdAsync_ReturnsData()
        {
            var options = CreateSqlServerContextOptions();

            using (var context = new DaDashboardDbContext(options))
            {
                // Use an existing record by retrieving one from the database.
                var existingEntity = await context.BusinessEntities.FirstOrDefaultAsync();
                Assert.IsNotNull(existingEntity, "There should be at least one BusinessEntity in the database for this test.");

                var repository = new BusinessEntityRepository(context);
                string? name = await repository.GetBusinessEntityNameByIdAsync(existingEntity.Id);

                Assert.IsNotNull(name, "The repository method should return a name.");
                Assert.AreEqual(existingEntity.Name, name, "The returned name should match the actual entity's name.");

                // Optionally, output the name for debugging.
                Console.WriteLine($"Retrieved name for BusinessEntity {existingEntity.Id}: {name}");
            }
        }
    }
}
