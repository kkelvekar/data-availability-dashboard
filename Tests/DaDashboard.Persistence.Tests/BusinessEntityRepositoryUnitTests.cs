using DaDashboard.Domain.Entities;
using DaDashboard.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DaDashboard.Persistence.Tests.Repositories
{
    [TestClass]
    public class BusinessEntityRepositoryUnitTests
    {
        /// <summary>
        /// Helper method to create new in-memory context options.
        /// </summary>
        /// <returns>A new instance of DbContextOptions</returns>
        private DbContextOptions<DaDashboardDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<DaDashboardDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Verifies that GetActiveBusinessEntitiesWithDetailsAsync returns only the active entities
        /// and that the navigation properties are correctly loaded.
        /// </summary>
        [TestMethod]
        public async Task GetActiveBusinessEntitiesWithDetailsAsync_ReturnsActiveEntities()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            Guid configId = Guid.NewGuid();
            Guid ragConfigId = Guid.NewGuid();
            Guid activeEntityId = Guid.NewGuid();
            Guid inactiveEntityId = Guid.NewGuid();

            using (var context = new DaDashboardDbContext(options))
            {
                // Create configuration objects
                var config = new BusinessEntityConfig
                {
                    Id = configId,
                    Name = "Test Config",
                    Metadata = "{\"key\": \"value\"}",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var ragConfig = new BusinessEntityRAGConfig
                {
                    Id = ragConfigId,
                    RedExpression = "RedRule",
                    AmberExpression = "AmberRule",
                    GreenExpression = "GreenRule",
                    Description = "Test RAG Config",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Create active entity
                var activeEntity = new BusinessEntity
                {
                    Id = activeEntityId,
                    ApplicationOwner = "Owner1",
                    Name = "Active Entity",
                    DisplayName = "Active Display",
                    DependentFunctionalities = "Func1,Func2",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                // Create inactive entity
                var inactiveEntity = new BusinessEntity
                {
                    Id = inactiveEntityId,
                    ApplicationOwner = "Owner2",
                    Name = "Inactive Entity",
                    DisplayName = "Inactive Display",
                    DependentFunctionalities = "Func3",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                context.BusinessEntityConfigs.Add(config);
                context.BusinessEntityRAGConfigs.Add(ragConfig);
                context.BusinessEntities.AddRange(activeEntity, inactiveEntity);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                IEnumerable<BusinessEntity> activeEntities = await repository.GetActiveBusinessEntitiesWithDetailsAsync();

                // Assert
                Assert.IsNotNull(activeEntities, "Returned collection should not be null.");
                var list = activeEntities.ToList();
                Assert.AreEqual(1, list.Count, "There should be exactly one active business entity.");
                Assert.AreEqual(activeEntityId, list[0].Id, "The active entity's ID should match the seeded data.");
                Assert.IsNotNull(list[0].BusinessEntityConfig, "BusinessEntityConfig should be loaded.");
                Assert.IsNotNull(list[0].BusinessEntityRAGConfig, "BusinessEntityRAGConfig should be loaded.");
            }
        }

        /// <summary>
        /// Verifies that GetActiveBusinessEntitiesWithDetailsAsync returns inactive entities
        /// when the parameter is passed as false.
        /// </summary>
        [TestMethod]
        public async Task GetActiveBusinessEntitiesWithDetailsAsync_ReturnsInactiveEntities_WhenIsActiveIsFalse()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            Guid configId = Guid.NewGuid();
            Guid ragConfigId = Guid.NewGuid();
            Guid inactiveEntityId = Guid.NewGuid();

            using (var context = new DaDashboardDbContext(options))
            {
                // Create configuration objects
                var config = new BusinessEntityConfig
                {
                    Id = configId,
                    Name = "Test Config",
                    Metadata = "{}",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var ragConfig = new BusinessEntityRAGConfig
                {
                    Id = ragConfigId,
                    RedExpression = "RedRule",
                    AmberExpression = "AmberRule",
                    GreenExpression = "GreenRule",
                    Description = "Test RAG Config",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Create inactive entity
                var inactiveEntity = new BusinessEntity
                {
                    Id = inactiveEntityId,
                    ApplicationOwner = "Owner2",
                    Name = "Inactive Entity",
                    DisplayName = "Inactive Display",
                    DependentFunctionalities = "Func3",
                    BusinessEntityConfigId = configId,
                    BusinessEntityRAGConfigId = ragConfigId,
                    IsActive = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BusinessEntityConfig = config,
                    BusinessEntityRAGConfig = ragConfig
                };

                context.BusinessEntityConfigs.Add(config);
                context.BusinessEntityRAGConfigs.Add(ragConfig);
                context.BusinessEntities.Add(inactiveEntity);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                // Pass false to get inactive entities.
                IEnumerable<BusinessEntity> inactiveEntities = await repository.GetActiveBusinessEntitiesWithDetailsAsync(false);

                // Assert
                Assert.IsNotNull(inactiveEntities, "Returned collection should not be null.");
                var list = inactiveEntities.ToList();
                Assert.AreEqual(1, list.Count, "There should be exactly one inactive business entity.");
                Assert.AreEqual(inactiveEntityId, list[0].Id, "The inactive entity's ID should match the seeded data.");
            }
        }

        /// <summary>
        /// Verifies that GetBusinessEntityNameByIdAsync returns the correct name when the record exists.
        /// </summary>
        [TestMethod]
        public async Task GetBusinessEntityNameByIdAsync_ReturnsCorrectName_WhenEntityExists()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            Guid testEntityId = Guid.NewGuid();
            string expectedName = "Test Entity Name";

            using (var context = new DaDashboardDbContext(options))
            {
                // Create related configuration entities.
                var config = new BusinessEntityConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Config For Name Test",
                    Metadata = "{}",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var ragConfig = new BusinessEntityRAGConfig
                {
                    Id = Guid.NewGuid(),
                    RedExpression = "Red",
                    AmberExpression = "Amber",
                    GreenExpression = "Green",
                    Description = "RAG Config For Name Test",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                var entity = new BusinessEntity
                {
                    Id = testEntityId,
                    ApplicationOwner = "Owner",
                    Name = expectedName,
                    DisplayName = "Display " + expectedName,
                    DependentFunctionalities = "None",
                    BusinessEntityConfigId = config.Id,
                    BusinessEntityRAGConfigId = ragConfig.Id,
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

            // Act
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                string? actualName = await repository.GetBusinessEntityNameByIdAsync(testEntityId);

                // Assert
                Assert.IsNotNull(actualName, "Returned name should not be null.");
                Assert.AreEqual(expectedName, actualName, "The returned entity name should match the expected value.");
            }
        }

        /// <summary>
        /// Verifies that GetBusinessEntityNameByIdAsync returns null when no entity is found for the given ID.
        /// </summary>
        [TestMethod]
        public async Task GetBusinessEntityNameByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            Guid nonExistingId = Guid.NewGuid();

            // Act
            using (var context = new DaDashboardDbContext(options))
            {
                var repository = new BusinessEntityRepository(context);
                string? actualName = await repository.GetBusinessEntityNameByIdAsync(nonExistingId);

                // Assert
                Assert.IsNull(actualName, "When the entity does not exist, the returned name should be null.");
            }
        }
    }
}

