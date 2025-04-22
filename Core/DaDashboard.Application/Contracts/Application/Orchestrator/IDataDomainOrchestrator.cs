using DaDashboard.Domain;

namespace DaDashboard.Application.Contracts.Application
{
    /// <summary>
    /// Defines a service that orchestrates domain operations, such as aggregating business entity summaries.
    /// </summary>
    public interface IDataDomainOrchestrator
    {
        /// <summary>
        /// Retrieves summary information for all business entities.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a collection of business entity summaries.</returns>
        Task<IEnumerable<BusinessEntitySummary>> GetBusinessEntitySummaryAsync();
    }
}