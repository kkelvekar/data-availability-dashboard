using DaDashboard.Domain;

namespace DaDashboard.Application.Contracts.Application
{
    public interface IDataDomainOrchestrator
    {
        IEnumerable<DataDomain> GetDataDomains(string date);
    }
}