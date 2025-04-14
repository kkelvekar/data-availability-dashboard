using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaDashboard.Persistence.Repositories
{
    public class BusinessEntityRepository : BaseRepository<BusinessEntity>, IBusinessEntityRepository
    {
        public BusinessEntityRepository(DaDashboardDbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        /// Retrieves all active Business Entities along with their associated BusinessEntityConfig and BusinessEntityRAGConfig details.
        /// </summary>
        /// <returns>A list of active BusinessEntity objects with navigation properties loaded.</returns>
        public async Task<IEnumerable<BusinessEntity>> GetActiveBusinessEntitiesWithDetailsAsync(bool IsActive = true)
        {
            return await _dbContext.BusinessEntities
                .Include(be => be.BusinessEntityConfig)
                .Include(be => be.BusinessEntityRAGConfig)
                .Where(be => be.IsActive == IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the Name property of a Business Entity based on its unique identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier for the Business Entity.</param>
        /// <returns>The Name of the Business Entity if found; otherwise, an empty string.</returns>
        public async Task<string?> GetBusinessEntityNameByIdAsync(Guid entityId)
        {
            return await _dbContext.BusinessEntities
                .Where(be => be.Id == entityId)
                .Select(be => be.Name)
                .FirstOrDefaultAsync();
        }

    }
}
