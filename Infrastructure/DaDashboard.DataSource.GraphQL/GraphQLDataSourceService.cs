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
            string baseUrl = GetBaseUrl(gqlConfig);
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
            List<Task<(string entityKey, IEnumerable<DataLoadMatrix> records)>> tasks;
            // Create tasks concurrently using LINQ. Each task calls GetDataLoadMatrixAsync and then pairs the entityKey with its result.
            if (metadata.entityKeys.Any())
            {
                tasks = metadata.entityKeys.Select(entityKey =>
                     _graphQLMetricsService.GetDataLoadMatrixAsync(
                         entityName: entityKey,
                         effectiveDate: effectiveDate,
                         baseUrl: baseUrl,
                         endpoint: endpoint)
                     .ContinueWith(task => (entityKey, records: task.Result))
                ).ToList();
            }
            else
            {
                tasks = new();
                tasks.Add(_graphQLMetricsService.GetDataLoadMatrixAsync(
                    entityName: null,
                    effectiveDate: effectiveDate,
                    baseUrl: baseUrl,
                    endpoint: endpoint)
                    .ContinueWith(task => (String.Empty, records: task.Result))
                );
            }

            // Await all tasks concurrently.
            var results = await Task.WhenAll(tasks);

            // Flatten all records and map each DataLoadMatrix record to a DataMetric,
            // setting the EntityKey from the tuple.
            var dataMetrics = results.SelectMany(result =>
                result.records.Select(record => new DataMetric
                {
                    EntityKey = result.entityKey,
                    Count = record.count,
                    Date = record.effectiveDate,
                    Message = record.Message
                })
            ).ToList();

            return dataMetrics;
        }

        private string GetBaseUrl(DomainSourceTypeGraphQL gqlConfig)
        {
            return gqlConfig.DevBaseUrl;
        }
    }
}
