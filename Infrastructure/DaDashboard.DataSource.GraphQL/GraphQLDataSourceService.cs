using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Application.Models.Infrastructure.GraphQL;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DaDashboard.DataSource.GraphQL
{
    // A simple model to deserialize the Metadata JSON.
    public class MetadataModel
    {
        public string[] entityKeys { get; set; }
    }

    public class GraphQLDataSourceService : IDataSourceService
    {
        private readonly IGraphQLDomainMetricsService _graphQLMetricsService;

        public GraphQLDataSourceService(IGraphQLDomainMetricsService graphQLMetricsService)
        {
            _graphQLMetricsService = graphQLMetricsService;
        }

        public string SourceType => "GraphQL";

        public async Task<List<DataMetric>> GetDataMetricAsync(DataDomainConfig config, DateTime? effectiveDate)
        {
            var gqlConfig = config.DomainSourceGraphQL;
            // Choose the proper URL based on your environment; here we use DevBaseUrl.
            string baseUrl = gqlConfig.DevBaseUrl;
            string endpoint = gqlConfig.EndpointPath;

            // Deserialize the metadata JSON; expected format: { "entityKeys": ["BENCHMARK", "FXRATE", ...] }
            MetadataModel metadata;
            try
            {
                metadata = JsonSerializer.Deserialize<MetadataModel>(gqlConfig.Metadata);
            }
            catch (Exception)
            {
                // Fallback to an empty array if deserialization fails.
                metadata = new MetadataModel { entityKeys = Array.Empty<string>() };
            }

            if (metadata?.entityKeys == null || metadata.entityKeys.Length == 0)
            {
                return new List<DataMetric>();
            }

            // Create tasks to call the GraphQL endpoint concurrently for each entity key.
            var tasks = new List<Task<IEnumerable<DataLoadMatrix>>>();
            foreach (var entityKey in metadata.entityKeys)
            {
                tasks.Add(_graphQLMetricsService.GetDataLoadMatrixAsync(
                    entityName: entityKey,
                    effectiveDate: effectiveDate,
                    baseUrl: baseUrl,
                    endpoint: endpoint));
            }

            // Await all tasks concurrently.
            var results = await Task.WhenAll(tasks);

            // Flatten all records into a single list.
            var allRecords = results.SelectMany(r => r).ToList();

            // Map each DataLoadMatrix record to a DataMetric.
            var dataMetrics = allRecords.Select(record => new DataMetric
            {
                Count = record.count,
                Date = record.effectiveDate
            }).ToList();

            return dataMetrics;
        }
    }
}
