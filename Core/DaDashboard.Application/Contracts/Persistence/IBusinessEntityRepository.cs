using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Contracts.Persistence
{
    /// <summary>
    /// Defines repository operations specific to BusinessEntity, extending the generic asynchronous repository.
    /// </summary>
    public interface IBusinessEntityRepository : IAsyncRepository<BusinessEntity>
    {
        /// <summary>
        /// Retrieves business entities filtered by active status, including their full details.
        /// </summary>
        /// <param name="IsActive">Indicates whether to include only active entities (default is true).</param>
        /// <returns>A collection of business entities with detailed information.</returns>
        Task<IEnumerable<BusinessEntity>> GetActiveBusinessEntitiesWithDetailsAsync(bool IsActive = true);
        /// <summary>
        /// Retrieves the name of a business entity given its unique identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier of the business entity.</param>
        /// <returns>The display name of the business entity if found; otherwise, null.</returns>
        Task<string?> GetBusinessEntityNameByIdAsync(Guid entityId);
    }
}
