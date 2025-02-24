using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    public class DataDomainOrchestratorTests
    {
        [Fact]
        public async Task GetDataDomainsAsync_ShouldReturnEmpty_WhenNoConfigsFound()
        {
            // Arrange
            var repositoryMock = new Mock<IDataDomainConfigRepository>();
            repositoryMock
                .Setup(r => r.GetAll(true))
                .ReturnsAsync(new List<DataDomainConfig>());

            var dataSourceServices = new List<IDataSourceService>();

            var orchestrator = new DataDomainOrchestrator(repositoryMock.Object, dataSourceServices);

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDataDomainsAsync_ShouldReturnDomains_WhenMatchingServiceExists()
        {
            // Arrange
            var config = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "Test Domain",
                SourceType = "GraphQL"
            };

            var metricList = new List<DataMetric>
            {
                new DataMetric() // add necessary property values if required
            };

            var repositoryMock = new Mock<IDataDomainConfigRepository>();
            repositoryMock
                .Setup(r => r.GetAll(true))
                .ReturnsAsync(new List<DataDomainConfig> { config });

            var dataSourceServiceMock = new Mock<IDataSourceService>();
            dataSourceServiceMock
                .Setup(s => s.SourceType)
                .Returns("GraphQL");
            dataSourceServiceMock
                .Setup(s => s.GetDataMetricAsync(config, It.IsAny<DateTime?>()))
                .ReturnsAsync(metricList);

            var dataSourceServices = new List<IDataSourceService> { dataSourceServiceMock.Object };

            var orchestrator = new DataDomainOrchestrator(repositoryMock.Object, dataSourceServices);

            // Act
            var effectiveDate = new DateTime(2020, 1, 1);
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate);

            // Assert
            result.Should().HaveCount(1);
            var domain = result.First();
            domain.Id.Should().Be(config.Id);
            domain.Name.Should().Be(config.DomainName);
            domain.Metrics.Should().BeEquivalentTo(metricList);

            // Verify that the effective date is passed along
            dataSourceServiceMock.Verify(s => s.GetDataMetricAsync(config, effectiveDate), Times.Once);
        }

        [Fact]
        public async Task GetDataDomainsAsync_ShouldSkipConfig_WhenNoMatchingServiceFound()
        {
            // Arrange
            var config = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "Test Domain",
                SourceType = "Kafka"
            };

            var repositoryMock = new Mock<IDataDomainConfigRepository>();
            repositoryMock
                .Setup(r => r.GetAll(true))
                .ReturnsAsync(new List<DataDomainConfig> { config });

            // Setup a service whose SourceType does NOT match the config's SourceType.
            var dataSourceServiceMock = new Mock<IDataSourceService>();
            dataSourceServiceMock
                .Setup(s => s.SourceType)
                .Returns("GraphQL");

            var dataSourceServices = new List<IDataSourceService> { dataSourceServiceMock.Object };

            var orchestrator = new DataDomainOrchestrator(repositoryMock.Object, dataSourceServices);

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDataDomainsAsync_ShouldReturnDomains_ForMultipleConfigs()
        {
            // Arrange
            var config1 = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "Domain 1",
                SourceType = "GraphQL"
            };

            var config2 = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "Domain 2",
                SourceType = "Kafka"
            };

            var metricList1 = new List<DataMetric> { new DataMetric() };
            var metricList2 = new List<DataMetric> { new DataMetric(), new DataMetric() };

            var repositoryMock = new Mock<IDataDomainConfigRepository>();
            repositoryMock
                .Setup(r => r.GetAll(true))
                .ReturnsAsync(new List<DataDomainConfig> { config1, config2 });

            var dataSourceServiceMock1 = new Mock<IDataSourceService>();
            dataSourceServiceMock1
                .Setup(s => s.SourceType)
                .Returns("GraphQL");
            dataSourceServiceMock1
                .Setup(s => s.GetDataMetricAsync(config1, It.IsAny<DateTime?>()))
                .ReturnsAsync(metricList1);

            var dataSourceServiceMock2 = new Mock<IDataSourceService>();
            dataSourceServiceMock2
                .Setup(s => s.SourceType)
                .Returns("Kafka");
            dataSourceServiceMock2
                .Setup(s => s.GetDataMetricAsync(config2, It.IsAny<DateTime?>()))
                .ReturnsAsync(metricList2);

            var dataSourceServices = new List<IDataSourceService>
            {
                dataSourceServiceMock1.Object,
                dataSourceServiceMock2.Object
            };

            var orchestrator = new DataDomainOrchestrator(repositoryMock.Object, dataSourceServices);

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            result.Should().HaveCount(2);

            var domain1 = result.FirstOrDefault(d => d.Id == config1.Id);
            var domain2 = result.FirstOrDefault(d => d.Id == config2.Id);

            domain1.Should().NotBeNull();
            domain1.Name.Should().Be(config1.DomainName);
            domain1.Metrics.Should().BeEquivalentTo(metricList1);

            domain2.Should().NotBeNull();
            domain2.Name.Should().Be(config2.DomainName);
            domain2.Metrics.Should().BeEquivalentTo(metricList2);
        }

        [Fact]
        public async Task GetDataDomainsAsync_ShouldThrow_WhenRepositoryReturnsNull()
        {
            // Arrange
            var repositoryMock = new Mock<IDataDomainConfigRepository>();
            // Simulate repository returning null instead of a list
            repositoryMock
                .Setup(r => r.GetAll(true))
                .ReturnsAsync((List<DataDomainConfig>)null);

            var orchestrator = new DataDomainOrchestrator(repositoryMock.Object, new List<IDataSourceService>());

            // Act
            Func<Task> act = async () => await orchestrator.GetDataDomainsAsync(null);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
