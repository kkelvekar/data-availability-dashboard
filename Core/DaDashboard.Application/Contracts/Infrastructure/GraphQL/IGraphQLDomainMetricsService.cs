using DaDashboard.Application.Models.Infrastructure.GraphQL;

namespace DaDashboard.Application.Contracts.Infrastructure.GraphQL
{
    public interface IGraphQLDomainMetricsService
    {
        Task<IEnumerable<DataLoadMatrix>> GetDataLoadMatrixAsync(string entityName, DateTime? effectiveDate, string baseUrl, string endpoint, CancellationToken cancellationToken = default);
    }
}