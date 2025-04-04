using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
namespace DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics
{
    public interface IJobStatsService
    {
        Task<List<JobStats>> GetJobStatsAsync(Models.Infrastructure.DataLoadStatistics.JobStatsRequest filter);
    }
}