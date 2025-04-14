using System;
using System.Collections.Generic;
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
    public class BusinessEntityRepositoryIntegrationTests
    {
        // Instance-level lists to keep track of inserted record IDs
        private readonly List<Guid> _businessEntityIds = new List<Guid>();
        private readonly List<Guid> _businessEntityConfigIds = new List<Guid>();
        private readonly List<Guid> _businessEntityRAGConfigIds = new List<Guid>();

        // Adjust the connection string as needed.
        private DbContextOptions<DaDashboardDbContext> CreateSqlServerContextOptions()
        {
            return new DbContextOptionsBuilder<DaDashboardDbContext>()
                .UseSqlServer("Server=KAUSTUBH-PC;Database=da-dashboard-db;Integrated Security=True;TrustServerCertificate=True;")
                .Options;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Clear out ID lists before each test.
            _businessEntityIds.Clear();
            _businessEntityConfigIds.Clear();
            _businessEntityRAGConfigIds.Clear();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            // Clean up the inserted test data.
            var options = CreateSqlServerContextOptions();
            using (var context = new DaDashboardDbContext(options))
            {
                // Remove BusinessEntities that were inserted during tests.
                foreach (var id in _businessEntityIds)
                {
                    var entity = await context.BusinessEntities.FindAsync(id);
                    if (entity != null)
                    {
                        context.BusinessEntities.Remove(entity);
                    }
                }

                // Remove BusinessEntityConfigs.
                foreach (var id in _businessEntityConfigIds)
                {
                    var config = await context.BusinessEntityConfigs.FindAsync(id);
                    if (config != null)
                    {
                        context.BusinessEntityConfigs.Remove(config);
                    }
                }

                // Remove BusinessEntityRAGConfigs.
                foreach (var id in _businessEntityRAGConfigIds)
                {
                    var ragConfig = await context.BusinessEntityRAGConfigs.FindAsync(id);
                    if (ragConfig != null)
                    {
                        context.BusinessEntityRAGConfigs.Remove(ragConfig);
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetActiveBusinessEntitiesWithDetailsAsync_ReturnsActiveEntitiesWithDetails()
        {
            // Arrange: create options to connect to the actual SQL Server database.
            var options = CreateSqlServerContextOptions();

            // For these integration tests, we seed test data.
            // It is recommended to use a dedicated test database or cleanup test data after the test.
            Guid configId = Guid.NewGuid();
            Guid ragConfigId = Guid.NewGuid();
            Guid activeEntityId = Guid.NewGuid();
            Guid inactiveEntityId = Guid.NewGuid();

            // Track inserted IDs
            _businessEntityConfigIds.Add(configId);
            _businessEntityRAGConfigIds.Add(ragConfigId);
            _businessEntityIds.Add(activeEntityId);
            _businessEntityIds.Add(inactiveEntityId);

            using (var context = new DaDashboardDbContext(options))
            {
                // Create related configuration entities.
                var config = new BusinessEntityConfig
                {
                    Id = configId,
                    Name = "Integration Test Config",
                    Metadata = "{ \"env\": \"test\" }",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var ragConfig = new BusinessEntityRAGConfig
                {
                    Id = ragConfigId,
                    RedExpression = "Red Condition",
                    AmberExpression = "Amber Condition",
                    GreenExpression = "Green Condition",
                    Description = "Integration Test RAG Config",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var activeEntity = new BusinessEntity
                {
                    Id = activeEntityId,
                    ApplicationOwner = "Integration Owner 1",
                    Name = "Active Integration Entity",
                    DisplayName = "Active Integration Display",
                    DependentFunctionalities = "Func1,Func2",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                var inactiveEntity = new BusinessEntity
                {
                    Id = inactiveEntityId,
                    ApplicationOwner = "Integration Owner 2",
                    Name = "Inactive Integration Entity",
                    DisplayName = "Inactive Integration Display",
                    DependentFunctionalities = "Func3",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                // Insert test data.
                context.BusinessEntityConfigs.Add(config);
                context.BusinessEntityRAGConfigs.Add(ragConfig);
                context.BusinessEntities.Add(activeEntity);
                context.BusinessEntities.Add(inactiveEntity);
                await context.SaveChangesAsync();
            }

            // Act: test the repository method using a new context instance.
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                IEnumerable<BusinessEntity> activeEntities = await repository.GetActiveBusinessEntitiesWithDetailsAsync();

                // Assert: we should retrieve only the active entity with configuration details loaded.
                Assert.IsNotNull(activeEntities, "Returned collection should not be null.");

                var activeEntityList = activeEntities.ToList();
                Assert.IsTrue(activeEntityList.Any(e => e.Id == activeEntityId),
                              "The active business entity inserted should be returned.");

                var testEntity = activeEntityList.First(e => e.Id == activeEntityId);
                Assert.IsNotNull(testEntity.BusinessEntityConfig, "BusinessEntityConfig should be loaded.");
                Assert.IsNotNull(testEntity.BusinessEntityRAGConfig, "BusinessEntityRAGConfig should be loaded.");
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBusinessEntityNameByIdAsync_ReturnsCorrectName()
        {
            // Arrange: create options to connect to the SQL Server.
            var options = CreateSqlServerContextOptions();
            Guid testEntityId = Guid.NewGuid();
            const string expectedName = "Integration Entity Name Test";

            // Track this test entity ID for cleanup.
            _businessEntityIds.Add(testEntityId);

            using (var context = new DaDashboardDbContext(options))
            {
                // Insert related configuration entries (required due to foreign key constraints).
                var configId = Guid.NewGuid();
                var ragConfigId = Guid.NewGuid();

                _businessEntityConfigIds.Add(configId);
                _businessEntityRAGConfigIds.Add(ragConfigId);

                var config = new BusinessEntityConfig
                {
                    Id = configId,
                    Name = "Integration Dummy Config",
                    Metadata = "{}",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var ragConfig = new BusinessEntityRAGConfig
                {
                    Id = ragConfigId,
                    RedExpression = "Red",
                    AmberExpression = "Amber",
                    GreenExpression = "Green",
                    Description = "Integration Dummy RAG Config",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var entity = new BusinessEntity
                {
                    Id = testEntityId,
                    ApplicationOwner = "Integration Test Owner",
                    Name = expectedName,
                    DisplayName = "Display " + expectedName,
                    DependentFunctionalities = "None",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                context.BusinessEntityConfigs.Add(config);
                context.BusinessEntityRAGConfigs.Add(ragConfig);
                context.BusinessEntities.Add(entity);
                await context.SaveChangesAsync();
            }

            // Act: use a new context instance to read the entity using the repository method.
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                string? actualName = await repository.GetBusinessEntityNameByIdAsync(testEntityId);

                // Assert: verify that the correct name is returned.
                Assert.IsNotNull(actualName, "Returned name should not be null.");
                Assert.AreEqual(expectedName, actualName, "The returned business entity name should match the expected value.");
            }
        }
    }
}
