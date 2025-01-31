using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL;
using DaDashboard.GraphQL.Consumer.Models;

namespace DaDashboard.GraphQL.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Create the GraphQL client
            using var graphQLClient = new GraphQLHttpClient(
                new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri("https://localhost:7204/graphql")
                },
                new SystemTextJsonSerializer()
            );

            // 2. Define the query + variables
            var query = @"
          query ($entityName: String!, $effectiveDate: DateTime) {
            dataLoadMatrix(entityName: $entityName, effectiveDate: $effectiveDate) {
              count
              effectiveDate
            }
          }
        ";

            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    entityName = "BENCHMARK",
                    effectiveDate = (DateTime?)null
                }
            };

            try
            {
                // 3. Send the query
                var response = await graphQLClient.SendQueryAsync<DataLoadMatrixResponse>(request, stoppingToken);

                // 4. Check for errors
                if (response.Errors != null && response.Errors.Any())
                {
                    foreach (var error in response.Errors)
                    {
                        _logger.LogError("GraphQL Error: {Message}", error.Message);
                    }
                }
                else
                {
                    // 5. Process the dataLoadMatrix array
                    if (response.Data?.dataLoadMatrix != null)
                    {
                        foreach (var item in response.Data.dataLoadMatrix)
                        {
                            _logger.LogInformation(
                                "Count = {Count}, EffectiveDate = {Date}",
                                item.count,
                                item.effectiveDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GraphQL endpoint.");
            }
        }
    }
}
