using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Contracts.Persistence
{
    public interface IBusinessEntityRepository : IAsyncRepository<BusinessEntity>
    {
        Task<IEnumerable<BusinessEntity>> GetActiveBusinessEntitiesWithDetailsAsync(bool IsActive = true);
        Task<string?> GetBusinessEntityNameByIdAsync(Guid entityId);
    }
}
