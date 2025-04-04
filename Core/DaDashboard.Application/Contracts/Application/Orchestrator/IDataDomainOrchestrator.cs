using DaDashboard.Domain;

namespace DaDashboard.Application.Contracts.Application
{
    public interface IDataDomainOrchestrator
    {
        Task<IEnumerable<BusinessEntitySummary>> GetBusinessEntitySummaryAsync();
        Task<IEnumerable<DataDomain>?> GetDataDomainsAsync(DateTime? effectiveDate);
    }
}