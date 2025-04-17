using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using System.Net.Http.Json;
using System.Text.Json;

namespace DaDashboard.DataLoadStatistics.Service
{
    /// <summary>
    /// Default implementation of <see cref="IJobStatsService"/>, invoking the remote JobStats HTTP API.
    /// </summary>
    public class JobStatsService : IJobStatsService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructs the service using the named "JobStatsClient" HttpClient.
        /// </summary>
        /// <param name="httpClientFactory">Factory to create named clients.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="httpClientFactory"/> is null.</exception>
        public JobStatsService(IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory is null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClient = httpClientFactory.CreateClient("JobStatsClient");
        }

        /// <summary>
        /// Fetches job statistics for the given filter and base URL.
        /// </summary>
        /// <param name="filter">Contains business entities and optional record date.</param>
        /// <param name="baseUrl">The base URL of the remote API.</param>
        /// <returns>List of <see cref="JobStats"/>; never null.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="filter"/> or <paramref name="baseUrl"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">If no business entities are specified.</exception>
        /// <exception cref="HttpRequestException">For network or non-200 HTTP responses.</exception>
        /// <exception cref="JsonException">If the response cannot be deserialized.</exception>
        public async Task<List<JobStats>> GetJobStatsAsync(JobStatsRequest filter, string baseUrl)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));
            if (filter.BusinessEntities is null || !filter.BusinessEntities.Any())
                throw new ArgumentException("At least one business entity must be provided.", nameof(filter.BusinessEntities));

            var endpoint = BuildUri(baseUrl, filter);

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<JobStats>>(endpoint, _jsonOptions);
                return result ?? new List<JobStats>();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"HTTP error calling '{endpoint}'.", ex);
            }
            catch (NotSupportedException ex)
            {
                throw new JsonException($"Response content type is not valid JSON at '{endpoint}'.", ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize JSON from '{endpoint}'.", ex);
            }
        }

        /// <summary>
        /// Builds the full request URI including query parameters.
        /// </summary>
        private static Uri BuildUri(string baseUrl, JobStatsRequest filter)
        {
            baseUrl = baseUrl.TrimEnd('/') + "/";
            var uriBuilder = new UriBuilder(new Uri(baseUrl))
            {
                Path = "api/JobStats",
                Query = string.Join("&",
                    filter.RecordAsOfDate.HasValue
                        ? new[] { $"RecordAsOfDate={Uri.EscapeDataString(filter.RecordAsOfDate.Value.ToString("o"))}" }
                        : Array.Empty<string>()
                )
            };

            var entityParams = filter.BusinessEntities
                .Select(e => $"BusinessEntities={Uri.EscapeDataString(e)}");

            var allParams = string.Join("&",
                new[] { uriBuilder.Query.TrimStart('?') }
                .Concat(entityParams)
                .Where(q => !string.IsNullOrEmpty(q))
            );

            uriBuilder.Query = allParams;
            return uriBuilder.Uri;
        }
    }
}
