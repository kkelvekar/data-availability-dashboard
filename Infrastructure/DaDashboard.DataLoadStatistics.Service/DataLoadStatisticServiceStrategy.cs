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
    /// using configuration metadata to determine the correct base URL.
    /// </summary>
    public class DataLoadStatisticServiceStrategy : IJobStatsStrategy
    {
        /// <summary>
        /// StrategyName of the strategy used for factory lookup.
        /// </summary>
        public string StrategyName => "Data Load Statistic Service";

        private readonly IJobStatsService _jobStatsService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Represents an environment block within the BusinessEntityConfig metadata.
        /// </summary>
        /// <param name="Name">Logical name of the environment (e.g. "Dev", "Prod").</param>
        /// <param name="BaseUrl">Base URL for the JobStats endpoint in this environment.</param>
        public record EnvironmentConfig(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("baseUrl")] string BaseUrl);

        /// <summary>
        /// Root metadata object for BusinessEntityConfig, containing multiple environments.
        /// </summary>
        /// <param name="Environments">List of environment-specific configurations.</param>
        public record BusinessEntityConfigMetadata(
            [property: JsonPropertyName("environments")] IReadOnlyList<EnvironmentConfig> Environments);

        /// <summary>
        /// Initializes a new instance of <see cref="DataLoadStatisticServiceStrategy"/>.
        /// </summary>
        /// <param name="jobStatsService">Underlying service to call the JobStats API.</param>
        /// <param name="hostEnvironment">Provides the current application environment name.</param>
        public DataLoadStatisticServiceStrategy(
            IJobStatsService jobStatsService,
            IHostEnvironment hostEnvironment)
        {
            _jobStatsService = jobStatsService ?? throw new ArgumentNullException(nameof(jobStatsService));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        /// <summary>
        /// Retrieves JobStats for a collection of business entities, without date filtering.
        /// </summary>
        /// <param name="entities">The set of <see cref="BusinessEntity"/> to fetch stats for.</param>
        /// <returns>A list of <see cref="JobStats"/> for the specified entities.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="entities"/> is empty.</exception>
        public async Task<List<JobStats>> GetJobStatsAsync(IEnumerable<BusinessEntity> entities)
        {
            var (request, baseUrl) = PrepareRequest(entities, null);
            var stats = await _jobStatsService.GetJobStatsAsync(request, baseUrl);
            return stats;
        }

        /// <summary>
        /// Retrieves JobStats for a single business entity on a specific as-of date.
        /// </summary>
        /// <param name="entity">The <see cref="BusinessEntity"/> to fetch stats for.</param>
        /// <param name="recordAsOfDate">The cutoff date to filter JobStats records.</param>
        /// <returns>A list of <see cref="JobStats"/> for the specified entity and date.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="recordAsOfDate"/> is in the future.</exception>
        public async Task<List<JobStats>> GetJobStatsAsync(BusinessEntity entity, DateTime recordAsOfDate)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            if (recordAsOfDate > DateTime.UtcNow)
                throw new ArgumentOutOfRangeException(nameof(recordAsOfDate),
                    "RecordAsOfDate cannot be in the future.");

            var (request, baseUrl) = PrepareRequest([entity], recordAsOfDate);
            var stats = await _jobStatsService.GetJobStatsAsync(request, baseUrl);
            return stats;
        }

        /// <summary>
        /// Prepares a <see cref="JobStatsRequest"/> and resolves the correct base URL
        /// from the entity configuration metadata.
        /// </summary>
        /// <param name="entities">The business entities to include in the request.</param>
        /// <param name="recordAsOfDate">Optional cutoff date for the request.</param>
        /// <returns>
        /// A tuple containing the constructed <see cref="JobStatsRequest"/> and the resolved base URL.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="entities"/> is empty.</exception>
        private (JobStatsRequest Request, string BaseUrl) PrepareRequest(
            IEnumerable<BusinessEntity> entities,
            DateTime? recordAsOfDate)
        {
            if (entities is null)
                throw new ArgumentNullException(nameof(entities));

            var list = entities.ToList();
            if (list.Count == 0)
                throw new ArgumentException("At least one BusinessEntity is required.", nameof(entities));

            var configJson = list.First().BusinessEntityConfig.Metadata;
            var metadata = ParseMetadata(configJson);

            var envName = _hostEnvironment.EnvironmentName ?? "Dev";
            var envConfig = metadata.Environments
                .FirstOrDefault(e => e.Name.Equals(envName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"Environment '{envName}' not found in BusinessEntityConfig.Metadata.");

            var request = new JobStatsRequest
            {
                BusinessEntities = list.Select(e => e.Name).ToList(),
                RecordAsOfDate = recordAsOfDate
            };

            return (request, envConfig.BaseUrl);
        }

        /// <summary>
        /// Parses the JSON metadata string into a <see cref="BusinessEntityConfigMetadata"/> instance.
        /// </summary>
        /// <param name="json">The raw JSON metadata from the business entity config.</param>
        /// <returns>The deserialized <see cref="BusinessEntityConfigMetadata"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the JSON is missing, invalid, or cannot be deserialized.</exception>
        private BusinessEntityConfigMetadata ParseMetadata(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("BusinessEntityConfig.Metadata is empty.");

            try
            {
                return JsonSerializer.Deserialize<BusinessEntityConfigMetadata>(json, _jsonOptions)
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
