using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Contracts.Application.Orchestrator
{
    /// <summary>
    /// Defines a strategy for retrieving job statistics for one or more business entities.
    /// Implementations are selected by name via <see cref="JobStatsStrategyFactory"/>.
    /// </summary>
    public interface IJobStatsStrategy
    {
        /// <summary>
        /// Gets the unique identifier for the strategy, used to select the correct implementation.
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Retrieves job statistics for a collection of business entities.
        /// </summary>
        /// <param name="businessEntities">The business entities to fetch job statistics for.</param>
        /// <returns>A list of <see cref="JobStats"/> for the specified entities.</returns>
        Task<List<JobStats>> GetJobStatsAsync(IEnumerable<BusinessEntity> businessEntities);
        /// <summary>
        /// Retrieves job statistics for a single business entity as of a specific date.
        /// </summary>
        /// <param name="entity">The business entity to fetch job statistics for.</param>
        /// <param name="recordAsOfDate">The date for which to retrieve statistics.</param>
        /// <returns>A list of <see cref="JobStats"/> for the given entity and date.</returns>
        Task<List<JobStats>> GetJobStatsAsync(BusinessEntity entity, DateTime recordAsOfDate);
    }
}
