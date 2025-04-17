using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Contracts.Application.Orchestrator
{
    public interface IJobStatsStrategy
    {
        /// <summary>
        /// Unique name used to look up this strategy in the factory.
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Given a set of BusinessEntity objects, fetches their JobStats.
        /// </summary>
        Task<List<JobStats>> GetJobStatsAsync(IEnumerable<BusinessEntity> businessEntities);
        Task<List<JobStats>> GetJobStatsAsync(BusinessEntity entity, DateTime recordAsOfDate);
    }
}
