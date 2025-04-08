using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using System.Net.Http;
using System.Net.Http.Json;

namespace DaDashboard.DataLoadStatistics.Service
{
    public class JobStatsService : IJobStatsService
    {
        private readonly HttpClient _httpClient;

        public JobStatsService(IHttpClientFactory httpClientFactory)
        {
            // Use a named HttpClient. Ensure "JobStatsClient" is configured (e.g., BaseAddress) in Startup/Program.
            _httpClient = httpClientFactory.CreateClient("JobStatsClient");
        }

        public async Task<List<JobStats>> GetJobStatsAsync(JobStatsRequest filter)
        {
            // Base endpoint for the API.
            string endpoint = "api/JobStats";
            var queryParams = new List<string>();

            // Append recordAsOfDate parameter if provided.
            if (filter.RecordAsOfDate.HasValue)
            {
                // Using ISO 8601 format for the date.
                queryParams.Add($"RecordAsOfDate={Uri.EscapeDataString(filter.RecordAsOfDate.Value.ToString("o"))}");
            }

            // Append each business entity as separate query parameters.
            if (filter.BusinessEntities != null && filter.BusinessEntities.Any())
            {
                foreach (var businessEntity in filter.BusinessEntities)
                {
                    queryParams.Add($"BusinessEntities={Uri.EscapeDataString(businessEntity)}");
                }
            }

            // Build the full endpoint with query string if any parameters exist.
            if (queryParams.Any())
            {
                endpoint += "?" + string.Join("&", queryParams);
            }

            // Issue the GET request and deserialize the response into a list of JobStats.
            var response = await _httpClient.GetFromJsonAsync<List<JobStats>>(endpoint);
            return response ?? new List<JobStats>();
        }
    }
}
