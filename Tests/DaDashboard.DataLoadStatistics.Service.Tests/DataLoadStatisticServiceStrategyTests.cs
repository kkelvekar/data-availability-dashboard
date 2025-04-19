using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.DataLoadStatistics.Service;
using DaDashboard.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DaDashboard.DataLoadStatistics.Service.Tests
{
    [TestClass]
    public class DataLoadStatisticServiceStrategyTests
    {
        private const string ValidMetadata =
            "{\"environments\":[{\"name\":\"Dev\",\"baseUrl\":\"http://dev\"},{\"name\":\"Prod\",\"baseUrl\":\"http://prod\"}]}";

        [TestMethod]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DataLoadStatisticServiceStrategy(null!, "Dev"));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_NullEntities_ThrowsArgumentNullException()
        {
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => strategy.GetJobStatsAsync((IEnumerable<BusinessEntity>)null!));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_EmptyEntities_ThrowsArgumentException()
        {
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => strategy.GetJobStatsAsync(new List<BusinessEntity>()));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_InvalidMetadata_ThrowsInvalidOperationException()
        {
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = "" }
            };
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => strategy.GetJobStatsAsync(new[] { entity }));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_InvalidJsonMetadata_ThrowsInvalidOperationException()
        {
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = "not json" }
            };
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => strategy.GetJobStatsAsync(new[] { entity }));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_EnvironmentNotFound_ThrowsInvalidOperationException()
        {
            var metadata = "{\"environments\":[{\"name\":\"QA\",\"baseUrl\":\"http://qa\"}]}";
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = metadata }
            };
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Prod");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => strategy.GetJobStatsAsync(new[] { entity }));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_SingleEntityWithFutureDate_ThrowsArgumentOutOfRangeException()
        {
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = ValidMetadata }
            };
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
                strategy.GetJobStatsAsync(entity, DateTime.UtcNow.AddDays(1)));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_SingleEntityNull_ThrowsArgumentNullException()
        {
            var mockService = new Mock<IJobStatsService>();
            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => strategy.GetJobStatsAsync((BusinessEntity)null!, DateTime.UtcNow));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_ValidEntities_CallsServiceAndReturnsResult()
        {
            // Arrange
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = ValidMetadata }
            };
            var expected = new List<JobStats> { new JobStats { BusinessEntity = "E1", RecordLoaded = 10, RecordAsOfDate = DateTime.Parse("2025-01-01T00:00:00Z") } };
            var mockService = new Mock<IJobStatsService>();
            mockService
                .Setup(s => s.GetJobStatsAsync(It.IsAny<JobStatsRequest>(), "http://dev"))
                .ReturnsAsync(expected);

            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");

            // Act
            var result = await strategy.GetJobStatsAsync(new[] { entity });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public async Task GetJobStatsAsync_SingleEntityValid_CallsServiceAndReturnsResult()
        {
            // Arrange
            var entity = new BusinessEntity
            {
                Name = "E1",
                BusinessEntityConfig = new BusinessEntityConfig { Metadata = ValidMetadata }
            };
            var expected = new List<JobStats> { new JobStats { BusinessEntity = "E1", RecordLoaded = 20, RecordAsOfDate = DateTime.Parse("2025-02-02T00:00:00Z") } };
            var mockService = new Mock<IJobStatsService>();
            mockService
                .Setup(s => s.GetJobStatsAsync(It.IsAny<JobStatsRequest>(), "http://dev"))
                .ReturnsAsync(expected);

            var strategy = new DataLoadStatisticServiceStrategy(mockService.Object, "Dev");

            // Act
            var result = await strategy.GetJobStatsAsync(entity, DateTime.Parse("2025-02-01T00:00:00Z"));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(expected, result);
        }
    }
}