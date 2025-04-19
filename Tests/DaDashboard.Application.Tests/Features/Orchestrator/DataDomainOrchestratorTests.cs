using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    [TestClass]
    public class DataDomainOrchestratorTests
    {
        [TestMethod]
        public async Task GetBusinessEntitySummaryAsync_ReturnsCorrectSummary()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var ragConfigId = Guid.NewGuid();
            var date1 = new DateTime(2023, 1, 1);
            var date2 = new DateTime(2023, 1, 2);
            var businessConfig = new BusinessEntityConfig
            {
                Id = configId,
                Name = "ConfigA",
                Metadata = string.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var ragConfig = new BusinessEntityRAGConfig
            {
                Id = ragConfigId,
                RedExpression = string.Empty,
                AmberExpression = string.Empty,
                GreenExpression = string.Empty,
                Description = string.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var entity = new BusinessEntity
            {
                Id = Guid.NewGuid(),
                ApplicationOwner = "Owner1",
                Name = "Entity1",
                DisplayName = "Entity1",
                DependentFunctionalities = "Func1, Func2",
                BusinessEntityConfigId = configId,
                BusinessEntityConfig = businessConfig,
                BusinessEntityRAGConfigId = ragConfigId,
                BusinessEntityRAGConfig = ragConfig,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var businessEntities = new List<BusinessEntity> { entity };

            var repoMock = new Mock<IBusinessEntityRepository>();
            repoMock.Setup(r => r.GetActiveBusinessEntitiesWithDetailsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(businessEntities);

            var strategyMock = new Mock<IJobStatsStrategy>();
            strategyMock.Setup(s => s.StrategyName).Returns("ConfigA");
            strategyMock
                .Setup(s => s.GetJobStatsAsync(It.IsAny<IEnumerable<BusinessEntity>>()))
                .ReturnsAsync(new List<JobStats>
                {
                    new JobStats { Id = Guid.NewGuid(), BusinessEntity = "Entity1", RecordAsOfDate = date1, RecordLoaded = 5 },
                    new JobStats { Id = Guid.NewGuid(), BusinessEntity = "Entity1", RecordAsOfDate = date2, RecordLoaded = 10 }
                });

            var factory = new JobStatsStrategyFactory(new[] { strategyMock.Object });
            var loggerMock = new Mock<ILogger<DataDomainOrchestrator>>();
            var orchestrator = new DataDomainOrchestrator(factory, repoMock.Object, loggerMock.Object);

            // Act
            var summaries = (await orchestrator.GetBusinessEntitySummaryAsync()).ToList();

            // Assert
            Assert.AreEqual(1, summaries.Count);
            var summary = summaries.First();
            Assert.AreEqual("Entity1", summary.BusinessEntity);
            Assert.AreEqual("Owner1", summary.ApplicationOwner);
            Assert.AreEqual(15, summary.TotalRecordsLoaded);
            Assert.AreEqual(date2, summary.LatestLoadDate);
            var dependentFuncs = summary.DependentFuncs.ToList();
            CollectionAssert.AreEqual(new List<string> { "Func1", "Func2" }, dependentFuncs);
        }

        [TestMethod]
        public async Task GetBusinessEntitySummaryAsync_ThrowsException_WhenStrategyNotFound()
        {
            // Arrange
            var config = new BusinessEntityConfig { Id = Guid.NewGuid(), Name = "UnknownStrategy", Metadata = string.Empty, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow };
            var ragConfig = new BusinessEntityRAGConfig { Id = Guid.NewGuid(), RedExpression = string.Empty, AmberExpression = string.Empty, GreenExpression = string.Empty, Description = string.Empty, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow };
            var entity = new BusinessEntity
            {
                Id = Guid.NewGuid(),
                ApplicationOwner = "Owner1",
                Name = "Entity1",
                DisplayName = "Entity1",
                DependentFunctionalities = "Func1",
                BusinessEntityConfigId = config.Id,
                BusinessEntityConfig = config,
                BusinessEntityRAGConfigId = ragConfig.Id,
                BusinessEntityRAGConfig = ragConfig,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var repoMock = new Mock<IBusinessEntityRepository>();
            repoMock.Setup(r => r.GetActiveBusinessEntitiesWithDetailsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(new List<BusinessEntity> { entity });

            var factory = new JobStatsStrategyFactory(Array.Empty<IJobStatsStrategy>());
            var orchestrator = new DataDomainOrchestrator(factory, repoMock.Object, Mock.Of<ILogger<DataDomainOrchestrator>>());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => orchestrator.GetBusinessEntitySummaryAsync());
        }
        
        [TestMethod]
        public async Task GetBusinessEntitySummaryAsync_ThrowsArgumentNullException_WhenRepositoryReturnsNull()
        {
            // Arrange
            var repoMock = new Mock<IBusinessEntityRepository>();
            repoMock.Setup(r => r.GetActiveBusinessEntitiesWithDetailsAsync(It.IsAny<bool>()))
                    .ReturnsAsync((IEnumerable<BusinessEntity>)null!);
            var factory = new JobStatsStrategyFactory(Array.Empty<IJobStatsStrategy>());
            var orchestrator = new DataDomainOrchestrator(factory, repoMock.Object, Mock.Of<ILogger<DataDomainOrchestrator>>());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => orchestrator.GetBusinessEntitySummaryAsync());
        }

        [TestMethod]
        public async Task GetBusinessEntitySummaryAsync_ReturnsSummaries_ForMultipleConfigGroups()
        {
            // Arrange
            var configA = new BusinessEntityConfig
            {
                Id = Guid.NewGuid(),
                Name = "ConfigA",
                Metadata = string.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var configB = new BusinessEntityConfig
            {
                Id = Guid.NewGuid(),
                Name = "ConfigB",
                Metadata = string.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var ragConfigCommon = new BusinessEntityRAGConfig
            {
                Id = Guid.NewGuid(),
                RedExpression = string.Empty,
                AmberExpression = string.Empty,
                GreenExpression = string.Empty,
                Description = string.Empty,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var entityA = new BusinessEntity
            {
                Id = Guid.NewGuid(),
                ApplicationOwner = "OwnerA",
                Name = "EntityA",
                DisplayName = "EntityA",
                DependentFunctionalities = "F1",
                BusinessEntityConfigId = configA.Id,
                BusinessEntityConfig = configA,
                BusinessEntityRAGConfigId = ragConfigCommon.Id,
                BusinessEntityRAGConfig = ragConfigCommon,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var entityB = new BusinessEntity
            {
                Id = Guid.NewGuid(),
                ApplicationOwner = "OwnerB",
                Name = "EntityB",
                DisplayName = "EntityB",
                DependentFunctionalities = "F2,F3",
                BusinessEntityConfigId = configB.Id,
                BusinessEntityConfig = configB,
                BusinessEntityRAGConfigId = ragConfigCommon.Id,
                BusinessEntityRAGConfig = ragConfigCommon,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var entities = new List<BusinessEntity> { entityA, entityB };

            var repoMock = new Mock<IBusinessEntityRepository>();
            repoMock.Setup(r => r.GetActiveBusinessEntitiesWithDetailsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(entities);

            var strategyA = new Mock<IJobStatsStrategy>();
            strategyA.Setup(s => s.StrategyName).Returns("ConfigA");
            var dateA = new DateTime(2023, 1, 1);
            strategyA.Setup(s => s.GetJobStatsAsync(It.IsAny<IEnumerable<BusinessEntity>>()))
                     .ReturnsAsync(new List<JobStats>
                     {
                         new JobStats { BusinessEntity = "EntityA", RecordAsOfDate = dateA, RecordLoaded = 1 }
                     });

            var strategyB = new Mock<IJobStatsStrategy>();
            strategyB.Setup(s => s.StrategyName).Returns("ConfigB");
            var dateB = new DateTime(2023, 2, 1);
            strategyB.Setup(s => s.GetJobStatsAsync(It.IsAny<IEnumerable<BusinessEntity>>()))
                     .ReturnsAsync(new List<JobStats>
                     {
                         new JobStats { BusinessEntity = "EntityB", RecordAsOfDate = dateB, RecordLoaded = 2 }
                     });

            var factory2 = new JobStatsStrategyFactory(new[] { strategyA.Object, strategyB.Object });
            var orchestrator2 = new DataDomainOrchestrator(factory2, repoMock.Object, Mock.Of<ILogger<DataDomainOrchestrator>>());

            // Act
            var summaries = (await orchestrator2.GetBusinessEntitySummaryAsync()).OrderBy(s => s.BusinessEntity).ToList();

            // Assert
            Assert.AreEqual(2, summaries.Count);

            var summaryAResult = summaries[0];
            Assert.AreEqual("EntityA", summaryAResult.BusinessEntity);
            Assert.AreEqual("OwnerA", summaryAResult.ApplicationOwner);
            Assert.AreEqual(1, summaryAResult.TotalRecordsLoaded);
            Assert.AreEqual(dateA, summaryAResult.LatestLoadDate);

            var summaryBResult = summaries[1];
            Assert.AreEqual("EntityB", summaryBResult.BusinessEntity);
            Assert.AreEqual("OwnerB", summaryBResult.ApplicationOwner);
            Assert.AreEqual(2, summaryBResult.TotalRecordsLoaded);
            Assert.AreEqual(dateB, summaryBResult.LatestLoadDate);
        }
    }
}