using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using Xunit;
using DaDashboard.Application.Contracts.Application.Orchestrator;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    public class DataDomainOrchestratorTests
    {
        [Theory]
        [NSubstituteAutoData]
        public async Task GetDataDomainsAsync_ShouldReturnEmpty_WhenNoConfigsFound(
            [Frozen] IDataDomainConfigRepository repository,
            Fixture fixture)
        {
            // Configure fixture to omit circular references.
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Arrange: Repository returns an empty list.
            repository.GetAll(true).Returns(new List<DataDomainConfig>());
            fixture.Register<IEnumerable<IDataSourceService>>(() => new List<IDataSourceService>());

            var orchestrator = fixture.Create<DataDomainOrchestrator>();

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [NSubstituteAutoData]
        public async Task GetDataDomainsAsync_ShouldReturnDomains_WhenMatchingServiceExists(
            [Frozen] IDataDomainConfigRepository repository,
            [Frozen] IDataSourceService dataSourceService,
            Fixture fixture)
        {
            // Configure fixture to omit circular references.
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Arrange
            var config = fixture.Create<DataDomainConfig>();
            var metricList = fixture.CreateMany<DataMetric>(1).ToList();

            // Customize the config for this test scenario.
            config.SourceType = "GraphQL";
            config.DomainName = "Test Domain";
            config.Id = Guid.NewGuid();

            repository.GetAll(true).Returns(new List<DataDomainConfig> { config });

            dataSourceService.SourceType.Returns("GraphQL");
            dataSourceService
                .GetDataMetricAsync(config, Arg.Any<DateTime?>())
                .Returns(Task.FromResult(metricList));

            fixture.Register<IEnumerable<IDataSourceService>>(() => new[] { dataSourceService });

            var orchestrator = fixture.Create<DataDomainOrchestrator>();
            var effectiveDate = new DateTime(2020, 1, 1);

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate);

            // Assert
            result.Should().HaveCount(1);
            var domain = result.First();
            domain.Id.Should().Be(config.Id);
            domain.Name.Should().Be(config.DomainName);
            domain.Metrics.Should().BeEquivalentTo(metricList);

            await dataSourceService.Received(1).GetDataMetricAsync(config, effectiveDate);
        }

        [Theory]
        [NSubstituteAutoData]
        public async Task GetDataDomainsAsync_ShouldSkipConfig_WhenNoMatchingServiceFound(
            [Frozen] IDataDomainConfigRepository repository,
            [Frozen] IDataSourceService dataSourceService,
            Fixture fixture)
        {
            // Configure fixture to omit circular references.
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Arrange
            var config = fixture.Create<DataDomainConfig>();
            config.SourceType = "Kafka";
            config.DomainName = "Test Domain";
            config.Id = Guid.NewGuid();

            repository.GetAll(true).Returns(new List<DataDomainConfig> { config });

            // Setup a service with a non-matching SourceType.
            dataSourceService.SourceType.Returns("GraphQL");

            fixture.Register<IEnumerable<IDataSourceService>>(() => new[] { dataSourceService });

            var orchestrator = fixture.Create<DataDomainOrchestrator>();

            // Act
            var result = await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [NSubstituteAutoData]
        public async Task GetDataDomainsAsync_ShouldReturnDomains_ForMultipleConfigs(
            [Frozen] IDataDomainConfigRepository repository,
            IDataSourceService dataSourceService1,
            IDataSourceService dataSourceService2,
            Fixture fixture)
        {
            // Configure fixture to omit circular references.
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Arrange: Create two distinct configs.
            var config1 = fixture.Create<DataDomainConfig>();
            var config2 = fixture.Create<DataDomainConfig>();
            config1.SourceType = "GraphQL";
            config1.DomainName = "Domain 1";
            config1.Id = Guid.NewGuid();

            config2.SourceType = "Kafka";
            config2.DomainName = "Domain 2";
            config2.Id = Guid.NewGuid();

            var metricList1 = fixture.CreateMany<DataMetric>(1).ToList();
            var metricList2 = fixture.CreateMany<DataMetric>(2).ToList();

            repository.GetAll(true).Returns(new List<DataDomainConfig> { config1, config2 });

            dataSourceService1.SourceType.Returns("GraphQL");
            dataSourceService1
                .GetDataMetricAsync(config1, Arg.Any<DateTime?>())
                .Returns(Task.FromResult(metricList1));

            dataSourceService2.SourceType.Returns("Kafka");
            dataSourceService2
                .GetDataMetricAsync(config2, Arg.Any<DateTime?>())
                .Returns(Task.FromResult(metricList2));

            fixture.Register<IEnumerable<IDataSourceService>>(() => new[] { dataSourceService1, dataSourceService2 });

            var orchestrator = fixture.Create<DataDomainOrchestrator>();

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

        [Theory]
        [NSubstituteAutoData]
        public async Task GetDataDomainsAsync_ShouldThrow_WhenRepositoryReturnsNull(
            [Frozen] IDataDomainConfigRepository repository,
            Fixture fixture)
        {
            // Configure fixture to omit circular references.
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Arrange: Simulate repository returning null.
            repository.GetAll(true).Returns((List<DataDomainConfig>)null);
            fixture.Register<IEnumerable<IDataSourceService>>(() => new List<IDataSourceService>());

            var orchestrator = fixture.Create<DataDomainOrchestrator>();

            // Act
            Func<Task> act = async () => await orchestrator.GetDataDomainsAsync(effectiveDate: null);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
