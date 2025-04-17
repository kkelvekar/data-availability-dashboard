using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain.Entities;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaDashboard.DataLoadStatistics.Service
{
    /// <summary>
    /// Strategy for fetching JobStats from the DataLoadStatisticService endpoint,
    /// using configuration metadata and the injected activeEnvironment to determine the base URL.
    /// </summary>
    public class DataLoadStatisticServiceStrategy : IJobStatsStrategy
    {
        /// <summary>
        /// Name of the strategy for factory lookup.
        /// </summary>
        public string StrategyName => "Data Load Statistic Service";

        private readonly IJobStatsService _jobStatsService;
        private readonly string _activeEnvironment;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Environment block within the metadata.
        /// </summary>
        public record EnvironmentConfig(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("baseUrl")] string BaseUrl);

        /// <summary>
        /// Root metadata object containing all environments.
        /// </summary>
        public record BusinessEntityConfigMetadata(
            [property: JsonPropertyName("environments")] IReadOnlyList<EnvironmentConfig> Environments);

        /// <summary>
        /// Initializes the strategy with the required service and the current environment.
        /// </summary>
        public DataLoadStatisticServiceStrategy(
            IJobStatsService jobStatsService,
            string activeEnvironment)
        {
            _jobStatsService = jobStatsService ?? throw new ArgumentNullException(nameof(jobStatsService));
            _activeEnvironment = activeEnvironment ?? "Dev";
        }

        /// <inheritdoc/>
        public async Task<List<JobStats>> GetJobStatsAsync(IEnumerable<BusinessEntity> entities)
        {
            var (request, baseUrl) = PrepareRequest(entities, null);
            var stats = await _jobStatsService.GetJobStatsAsync(request, baseUrl);
            return stats;
        }

        /// <inheritdoc/>
        public async Task<List<JobStats>> GetJobStatsAsync(BusinessEntity entity, DateTime recordAsOfDate)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            if (recordAsOfDate > DateTime.UtcNow)
                throw new ArgumentOutOfRangeException(nameof(recordAsOfDate),
                    "RecordAsOfDate cannot be in the future.");

            var (request, baseUrl) = PrepareRequest(new[] { entity }, recordAsOfDate);
            var stats = await _jobStatsService.GetJobStatsAsync(request, baseUrl);
            return stats;
        }

        /// <summary>
        /// Builds the request and resolves the base URL from metadata.
        /// </summary>
        private (JobStatsRequest Request, string BaseUrl) PrepareRequest(
            IEnumerable<BusinessEntity> entities,
            DateTime? recordAsOfDate)
        {
            if (entities is null)
                throw new ArgumentNullException(nameof(entities));

            var list = entities.ToList();
            if (!list.Any())
                throw new ArgumentException("At least one BusinessEntity is required.", nameof(entities));

            var metadataJson = list[0].BusinessEntityConfig.Metadata;
            var metadata = ParseMetadata(metadataJson);

            var envConfig = metadata.Environments
                .FirstOrDefault(e => e.Name.Equals(_activeEnvironment, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"Environment '{_activeEnvironment}' not found in BusinessEntityConfig.Metadata.");

            var request = new JobStatsRequest
            {
                BusinessEntities = list.Select(e => e.Name).ToList(),
                RecordAsOfDate = recordAsOfDate
            };

            return (request, envConfig.BaseUrl);
        }

        /// <summary>
        /// Deserializes the metadata JSON into a POCO.
        /// </summary>
        private BusinessEntityConfigMetadata ParseMetadata(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("BusinessEntityConfig.Metadata is empty.");

            try
            {
                return JsonSerializer
                    .Deserialize<BusinessEntityConfigMetadata>(json, _jsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize metadata JSON.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Invalid JSON in BusinessEntityConfig.Metadata.", ex);
            }
        }
    }
}
