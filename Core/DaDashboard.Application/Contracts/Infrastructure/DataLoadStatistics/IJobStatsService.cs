using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
namespace DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics
{
    /// <summary>
    /// Defines a service to fetch job statistics from an external data source based on filter criteria.
    /// </summary>
    public interface IJobStatsService
    {
        /// <summary>
        /// Retrieves job statistics based on the specified request filter and API base URL.
        /// </summary>
        /// <param name="filter">The request containing filter parameters such as business entities and date.</param>
        /// <param name="baseURL">The base URL of the downstream job statistics API.</param>
        /// <returns>A list of <see cref="JobStats"/> that match the filter criteria.</returns>
        Task<List<JobStats>> GetJobStatsAsync(JobStatsRequest filter, string baseURL);
    }
}