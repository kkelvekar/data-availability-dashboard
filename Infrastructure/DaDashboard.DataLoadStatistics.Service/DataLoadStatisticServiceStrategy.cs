using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain.Entities;

namespace DaDashboard.DataLoadStatistics.Service
{
    public class DataLoadStatisticServiceStrategy : IJobStatsStrategy
    {
        public string Name => "Data Load Statistic Service";

        private readonly IJobStatsService _jobStatsService;
        private readonly IConfiguration _configuration;

        // POCO records for JSON mapping
        private record BusinessEntityConfigMetadata(
            [property: JsonPropertyName("environments")] IReadOnlyList<EnvironmentConfig> Environments);

        private record EnvironmentConfig(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("baseUrl")] string BaseUrl);

        public DataLoadStatisticServiceStrategy(
            IJobStatsService jobStatsService,
            IConfiguration configuration)
        {
            _jobStatsService = jobStatsService;
            _configuration = configuration;
        }

        public async Task<List<JobStats>> GetJobStatsAsync(IEnumerable<BusinessEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (!entities.Any())
                throw new ArgumentException("At least one BusinessEntity is required.", nameof(entities));

            // Use LINQ to extract metadata JSON from the first entity
            var metadataJson = entities
                .Select(e => e.BusinessEntityConfig.Metadata)
                .First();

            var environmentName = GetEnvironmentName();
            var metadata = ParseMetadata(metadataJson);
            var envConfig = FindEnvironmentConfig(metadata, environmentName);
            var request = BuildJobStatsRequest(entities);

            return await _jobStatsService.GetJobStatsAsync(request, envConfig.BaseUrl);
        }

        private string GetEnvironmentName()
        {
            return _configuration["ASPNETCORE_ENVIRONMENT"]
                ?? _configuration["CurrentEnvironment"]
                ?? "Dev";
        }

        private BusinessEntityConfigMetadata ParseMetadata(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("BusinessEntityConfig.Metadata is empty.");

            try
            {
                return JsonSerializer.Deserialize<BusinessEntityConfigMetadata>(json)
                    ?? throw new InvalidOperationException("Failed to deserialize metadata JSON.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Invalid JSON in BusinessEntityConfig.Metadata.", ex);
            }
        }

        private EnvironmentConfig FindEnvironmentConfig(BusinessEntityConfigMetadata metadata, string envName)
        {
            var config = metadata.Environments
                .FirstOrDefault(e => e.Name.Equals(envName, StringComparison.OrdinalIgnoreCase));

            if (config is null)
                throw new InvalidOperationException($"Environment '{envName}' not found in metadata.");

            return config;
        }

        private JobStatsRequest BuildJobStatsRequest(IEnumerable<BusinessEntity> entities)
        {
            return new JobStatsRequest
            {
                BusinessEntities = entities
                    .Select(e => e.Name)
                    .ToList()
            };
        }
    }
}