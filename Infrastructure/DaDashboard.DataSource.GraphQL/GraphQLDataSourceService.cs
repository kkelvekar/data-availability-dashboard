using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.DataSource.GraphQL
{
    public class GraphQLDataSourceService : IDataSourceService
    {
        private readonly IGraphQLDomainMetricsService _graphQLMetricsService;

        public GraphQLDataSourceService(IGraphQLDomainMetricsService graphQLMetricsService)
        {
            _graphQLMetricsService = graphQLMetricsService;
        }

        // This service handles GraphQL source type
        public string SourceType => "GraphQL";

        public async Task<DataMetric> GetDataMetricAsync(DataDomainConfig config, DateTime? effectiveDate)
        {
            var gqlConfig = config.DomainSourceGraphQL;
            // To Do: In a real app, choose the proper URL based on the environment (Dev, QA, PreProd, Prod). For this example, we use ProdBaseUrl.
            string baseUrl = gqlConfig.DevBaseUrl;
            string endpoint = gqlConfig.EndpointPath;
            string entityName = gqlConfig.EntityKey;

            // Call the GraphQL endpoint using the existing service
            var records = await _graphQLMetricsService.GetDataLoadMatrixAsync(
                entityName: entityName,
                effectiveDate: effectiveDate,
                baseUrl: baseUrl,
                endpoint: endpoint);

            var record = records.FirstOrDefault();
            if (record != null)
            {
                return new DataMetric
                {
                    Count = record.count,
                    Date = record.effectiveDate
                };
            }

            // Return a default metric if no data is found
            return new DataMetric
            {
                Count = 0,
                Date = DateTime.MinValue
            };
        }
    }
}
