using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Application.Models.Infrastructure.GraphQL;
using DaDashboard.DataSource.GraphQL.Helpers;
using DaDashboard.DataSource.GraphQL.Models;
using Microsoft.Extensions.Logging;

namespace DaDashboard.DataSource.GraphQL.Services
{
    /// <summary>
    /// A specialized service that queries the dataLoadMatrix field in GraphQL schema.
    /// </summary>
    public class GraphQLDomainMetricsService : IGraphQLDomainMetricsService
    {
        private readonly GraphQLClientService _graphQLClientService;
        private readonly ILogger<GraphQLDomainMetricsService> _logger;

        // Query that includes effectiveDate
        private const string DataLoadMatrixQuery = @"
          query ($entityName: String!, $effectiveDate: DateTime) {
            dataLoadMatrix(entityName: $entityName, effectiveDate: $effectiveDate) {
              count
              effectiveDate
            }
          }
        ";

        // Query without effectiveDate
        private const string DataLoadMatrixQueryWithoutEffectiveDate = @"
          query ($entityName: String!) {
            dataLoadMatrix(entityName: $entityName) {
              count
              effectiveDate
            }
          }
        ";


        public GraphQLDomainMetricsService(GraphQLClientService graphQLClientService, ILogger<GraphQLDomainMetricsService> logger)
        {
            _graphQLClientService = graphQLClientService;
            _logger = logger;
        }

        /// <summary>
        /// Calls the dataLoadMatrix field with the given arguments and returns the results.
        /// </summary>
        /// <param name="entityName">The entity name (required).</param>
        /// <param name="effectiveDate">Optional date filter.</param>
        /// <param name="baseUrl">Base URL (e.g. https://localhost:7204).</param>
        /// <param name="endpoint">Endpoint path (e.g. graphql or one without effective date).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of DataLoadMatrix records.</returns>
        public async Task<IEnumerable<DataLoadMatrix>> GetDataLoadMatrixAsync(
            string entityName,
            DateTime? effectiveDate,
            string baseUrl,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the appropriate query and variables using helper methods.
                var (query, variables) = GetQueryAndVariables(entityName, effectiveDate, endpoint);

                // Call the generic GraphQL client
                var response = await _graphQLClientService.SendQueryAsync<DataLoadMatrixResponse>(
                    query, variables, baseUrl, endpoint, cancellationToken);

                // Check for any GraphQL errors
                if (response.Errors != null && response.Errors.Any())
                {
                    var errorMessages = string.Join("; ", response.Errors.Select(e => e.Message));
                    _logger.LogError("GraphQL returned errors for entity {entityName} on endpoint {endpoint}: {errorMessages}",
                        entityName, endpoint, errorMessages);
                    // Return an empty collection to allow processing to continue.
                    return Array.Empty<DataLoadMatrix>();
                }

                // Return the dataLoadMatrix array (or empty if null)
                return response.Data?.dataLoadMatrix ?? Array.Empty<DataLoadMatrix>();
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions and return an empty result so that processing continues.
                _logger.LogError(ex, "Error occurred while fetching data load matrix for entity {entityName} on endpoint {endpoint}",
                    entityName, endpoint);
                return Array.Empty<DataLoadMatrix>();
            }
        }

        /// <summary>
        /// Combines the query and variables based on the endpoint condition.
        /// </summary>
        /// <param name="entityName">The entity name.</param>
        /// <param name="effectiveDate">The effective date filter.</param>
        /// <param name="endpoint">The endpoint to determine behavior.</param>
        /// <returns>A tuple containing the query string and the query variables.</returns>
        private (string query, object variables) GetQueryAndVariables(string entityName, DateTime? effectiveDate, string endpoint)
        {
            var variables = BuildQueryVariables(entityName, effectiveDate, endpoint);
            var query = GetQueryBasedOnEndpoint(endpoint);
            return (query, variables);
        }

        /// <summary>
        /// Builds the variables object for the GraphQL query.
        /// Depending on the endpoint, it conditionally includes the effectiveDate parameter.
        /// </summary>
        /// <param name="entityName">The entity name.</param>
        /// <param name="effectiveDate">The effective date filter.</param>
        /// <param name="endpoint">The endpoint name to determine behavior.</param>
        /// <returns>An object with the query variables.</returns>
        private object BuildQueryVariables(string entityName, DateTime? effectiveDate, string endpoint)
        {
            // If the endpoint indicates that the effective date should be omitted,
            // then only include the entityName in the query variables.
            if (!string.IsNullOrWhiteSpace(endpoint) &&
                endpoint.Contains("NoEffectiveDate", StringComparison.OrdinalIgnoreCase))
            {
                return new { entityName };
            }

            // For other endpoints, include both entityName and effectiveDate.
            return new { entityName, effectiveDate };
        }

        /// <summary>
        /// Returns the appropriate GraphQL query based on the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to determine behavior.</param>
        /// <returns>A GraphQL query string.</returns>
        private string GetQueryBasedOnEndpoint(string endpoint)
        {
            // If the endpoint indicates that the effective date should be omitted, use the query without effectiveDate.
            if (!string.IsNullOrWhiteSpace(endpoint) &&
                endpoint.Contains("NoEffectiveDate", StringComparison.OrdinalIgnoreCase))
            {
                return DataLoadMatrixQueryWithoutEffectiveDate;
            }

            // Otherwise, return the default query which includes effectiveDate.
            return DataLoadMatrixQuery;
        }
    }
}
