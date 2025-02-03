using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace DaDashboard.DataSource.GraphQL.Helpers
{
    /// <summary>
    /// A generic service to send GraphQL queries/mutations to any endpoint.
    /// </summary>
    public class GraphQLClientService
    {
        /// <summary>
        /// Sends a GraphQL query with variables to a given base URL and endpoint.
        /// </summary>
        /// <typeparam name="TResponse">Type to deserialize the 'data' portion of the response.</typeparam>
        /// <param name="query">The GraphQL query string.</param>
        /// <param name="variables">An anonymous object or dictionary representing GraphQL variables.</param>
        /// <param name="baseUrl">Base URL (e.g. https://localhost:7204).</param>
        /// <param name="endpoint">Endpoint path (e.g. graphql or /graphql).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A GraphQLResponse object containing 'data' and 'errors'.</returns>
        public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(
            string query,
            object variables,
            string baseUrl,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            // Combine baseUrl + endpoint safely
            var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            // Create a one-off GraphQL client
            using var client = new GraphQLHttpClient(
                new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri(url)
                },
                new SystemTextJsonSerializer()
            );

            // Build the request
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = variables
            };

            // Send the query to the server, expecting TResponse shape in "data"
            return await client.SendQueryAsync<TResponse>(request, cancellationToken)
                               .ConfigureAwait(false);
        }
    }
}

