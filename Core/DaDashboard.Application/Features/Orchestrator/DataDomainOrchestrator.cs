using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly IGraphQLDomainMetricsService _graphQLMetricsService;

        public DataDomainOrchestrator(IGraphQLDomainMetricsService graphQLMetricsService)
        {
            _graphQLMetricsService = graphQLMetricsService;
        }

        public async Task<IEnumerable<DataDomain>> GetDataDomainsAsync(string date)
        {
            // Parse the date if needed. If empty, pass null
            DateTime? effectiveDate = string.IsNullOrWhiteSpace(date)
                ? null
                : DateTime.Parse(date);

            // Call the dataLoadMatrix field
            var dataLoadMatrixRecords = await _graphQLMetricsService.GetDataLoadMatrixAsync(
                entityName: "BENCHMARK",
                effectiveDate: effectiveDate,
                baseUrl: "https://localhost:7204",  // or read from config
                endpoint: "graphql"                 // or read from config
            );

            var dataLoadMatrixRecord = dataLoadMatrixRecords.FirstOrDefault();

            // Map the GraphQL result to your domain models
            var dataDomain1 =  new DataDomain
            {
                Name = "BENCHMARK",
                LoadDate = dataLoadMatrixRecord.effectiveDate,
                Count = dataLoadMatrixRecord.count
            };

            var result = new List<DataDomain> { dataDomain1 };

            return result;
        }
    }
}
