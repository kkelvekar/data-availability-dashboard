using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Application.Models.Infrastructure.GraphQL;
using DaDashboard.DataSource.GraphQL.Helpers;
using DaDashboard.DataSource.GraphQL.Models;

namespace DaDashboard.DataSource.GraphQL.Services
{
    /// <summary>
    /// A specialized service that queries the dataLoadMatrix field in GraphQL schema.
    /// </summary>
    public class GraphQLDomainMetricsService : IGraphQLDomainMetricsService
    {
        private readonly GraphQLClientService _graphQLClientService;

        // The query we will send to the server
        private const string DataLoadMatrixQuery = @"
          query ($entityName: String!, $effectiveDate: DateTime) {
            dataLoadMatrix(entityName: $entityName, effectiveDate: $effectiveDate) {
              count
              effectiveDate
            }
          }
        ";

        public GraphQLDomainMetricsService(GraphQLClientService graphQLClientService)
        {
            _graphQLClientService = graphQLClientService;
        }

        /// <summary>
        /// Calls the dataLoadMatrix field with the given arguments and returns the results.
        /// </summary>
        /// <param name="entityName">The entity name (required).</param>
        /// <param name="effectiveDate">Optional date filter.</param>
        /// <param name="baseUrl">Base URL (e.g. https://localhost:7204).</param>
        /// <param name="endpoint">Endpoint path (e.g. graphql).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of DataLoadMatrix records.</returns>
        public async Task<IEnumerable<DataLoadMatrix>> GetDataLoadMatrixAsync(
            string entityName,
            DateTime? effectiveDate,
            string baseUrl,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            var variables = new
            {
                entityName,
                effectiveDate
            };

            // Call the generic GraphQL client
            var response = await _graphQLClientService.SendQueryAsync<DataLoadMatrixResponse>(DataLoadMatrixQuery,variables,baseUrl,endpoint,cancellationToken);

            // Check for any GraphQL errors
            if (response.Errors != null && response.Errors.Any())
            {
                // You could throw, log, or handle errors as needed
                var errorMessages = string.Join("; ", response.Errors.Select(e => e.Message));
                throw new Exception($"GraphQL returned errors: {errorMessages}");
            }

            // Return the dataLoadMatrix array (or empty if null)
            return response.Data?.dataLoadMatrix ?? Array.Empty<DataLoadMatrix>();
        }
    }
}
