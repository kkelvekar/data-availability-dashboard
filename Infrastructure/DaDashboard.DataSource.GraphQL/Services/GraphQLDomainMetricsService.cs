using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Application.Models.Infrastructure.GraphQL;
using DaDashboard.DataSource.GraphQL.Helpers;
using DaDashboard.DataSource.GraphQL.Models;
using GraphQL.Client.Http;
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

        private const string DataLoadMatrixQueryEntityNameEffDate = @"
            query ($entityName: String!, $effectiveDate: DateTime!) {
                dataLoadMatrix(entityName: $entityName, effectiveDate: $effectiveDate) {
                    count
                    effectiveDate
                }
            }";

        // EffectiveDate but no entity name
        private const string DataLoadMatrixQueryEffDate = @"
            query ($effectiveDate: DateTime!) {
                dataLoadMatrix(effectiveDate: $effectiveDate) {
                    count
                    effectiveDate
                }
            }";

        // Query without effectiveDate
        private const string DataLoadMatrixQueryEntityName = @"
            query ($entityName: String!) {
                dataLoadMatrix(entityName: $entityName) {
                    count
                    effectiveDate
                }
            }";

        private const string DataLoadMatrixQueryNoParams = @"
            query {
                dataLoadMatrix() {
                    count
                    effectiveDate
                }
            }";


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
                // Get the appropriate query and variables using helper methods
                var existence = GetParameterExistence(entityName, effectiveDate, endpoint);
                var (query, variables) = GetQueryAndVariables(entityName, effectiveDate, existence);

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
            catch (GraphQLHttpRequestException gex)
            {
                // Log the exception and return an empty result so that processing continues.
                _logger.LogError(gex, "GraphQL Error occurred while fetching data load matrix for entity {entityName} on endpoint {endpoint}", entityName, endpoint);
                return new[]
                {
                    new DataLoadMatrix
                    {
                        effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : default,
                        entityKey = entityName,
                        Message = $"GraphQL Error occurred while fetching data load matrix for entity {entityName} on endpoint {endpoint} : {gex.Message} ({gex.Content})"
                    }
                };
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions and return an empty result so that processing continues.
                _logger.LogError(ex, "Error occurred while fetching data load matrix for entity {entityName} on endpoint {endpoint}",
                    entityName, endpoint);
                return new[]
               {
                    new DataLoadMatrix
                    {
                        effectiveDate = effectiveDate.HasValue ? effectiveDate.Value : default,
                        entityKey = entityName,
                        Message = $"Error occurred while fetching data load matrix for entity {entityName} on endpoint {endpoint} : {ex.Message}"
                    }
                };
            }
        }


        /// <summary>
        /// Combines the query and variables based on the endpoint condition.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="existence"></param>
        private (string query, object variables) GetQueryAndVariables(string? entityName, DateTime? effectiveDate, ParameterExistence existence)
        {
            var variables = BuildQueryVariables(entityName, effectiveDate, existence);
            var query = GetQuery(existence);
            return (query, variables);
        }

        /// <summary>
        /// Work out which parameters exist by means of somewhat opaque process.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private ParameterExistence GetParameterExistence(string? entityName, DateTime? effectiveDate, string endpoint)
        {
            ParameterExistence entityNameExists = QueryHasNoEntityName(endpoint, entityName)
                ? ParameterExistence.None
                : ParameterExistence.EntityName;

            ParameterExistence effDateExists = QueryHasNoEffectiveDate(endpoint, effectiveDate)
                ? ParameterExistence.None
                : ParameterExistence.EffectiveDate;

            return entityNameExists | effDateExists;
        }

        /// <summary>
        /// Builds the variables object for the GraphQL query.
        /// </summary>
        /// <param name="entityNameNullable"></param>
        /// <param name="effectiveDateNullable"></param>
        /// <param name="existence"></param>
        /// <returns></returns>
        private object BuildQueryVariables(string? entityNameNullable, DateTime? effectiveDateNullable, ParameterExistence existence)
        {
            switch (existence)
            {
                case ParameterExistence.None:
                    // No parameters at all, return empty block
                    return new { };

                case ParameterExistence.EntityName:
                    {
                        // EntityName only
                        string entityName = entityNameNullable!;
                        return new { entityName };
                    }

                case ParameterExistence.EffectiveDate:
                    {
                        // EffectiveDate only
                        DateTime effectiveDate = effectiveDateNullable!.Value;
                        return new { effectiveDate }; // Fragile method - this anonymous type MUST have name effectiveDate
                    }

                case ParameterExistence.EntityNameAndEffDate:
                    {
                        // Has entityName and effectiveDate
                        string entityName = entityNameNullable!;
                        DateTime effectiveDate = effectiveDateNullable!.Value;
                        return new { entityName, effectiveDate };
                    }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the correct query to match which variables we have.
        /// TODO: It should be possible to build the query programmatically, but the GraphQL layer is different for each endpoint.
        /// </summary>
        private string GetQuery(ParameterExistence existence) =>
            existence switch
            {
                ParameterExistence.None => DataLoadMatrixQueryNoParams,
                ParameterExistence.EntityName => DataLoadMatrixQueryEntityName,
                ParameterExistence.EffectiveDate => DataLoadMatrixQueryEffDate,
                ParameterExistence.EntityNameAndEffDate => DataLoadMatrixQueryEntityNameEffDate,
                _ => throw new ArgumentOutOfRangeException(nameof(existence), existence, null)
            };

        /// <summary>
        /// Given the endpoint and the (nullable) effective date, determine whether the QueryHasNoEffectiveDate.
        /// WARNING! Portfolios endpoint NEVER has an effective date even if you try to give it one.
        /// TODO: Looking for the word "portfolios" is horrible and brittle; we need a better solution.
        /// Note: Endpoint is not nullable, so we don't need the first test against null or whitespace.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="effectiveDate"></param>
        /// <returns></returns>
        private static bool QueryHasNoEffectiveDate(string endpoint, DateTime? effectiveDate) =>
            endpoint.Contains("portfolios", StringComparison.OrdinalIgnoreCase) || effectiveDate.HasValue;

        /// <summary>
        /// Same again
        /// </summary>
        private static bool QueryHasNoEntityName(string endpoint, string? entityName) => entityName == null;

    }
}
