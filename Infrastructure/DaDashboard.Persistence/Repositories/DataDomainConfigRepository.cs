using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Persistence.Repositories
{
    public class DataDomainConfigRepository : BaseRepository<DataDomainConfig>, IDataDomainConfigRepository
    {
        public DataDomainConfigRepository(DaDashboardDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<List<DataDomainConfig>> GetAll(bool isActive)
        {
            return await _dbContext.DataDomainConfigs
                 .Include(d => d.DomainSourceGraphQL)  // Eager load the child entity
                 .Where(d => d.IsActive == isActive)   // Filter by active status
                 .AsNoTracking()                       // Good for read-only operations
                 .ToListAsync();
        }
    }
}

//SELECT
//    d.Id,
//    d.DomainName,
//    d.SourceType,
//    d.IsActive,
//    d.CreatedDate,
//    d.UpdatedDate,
//    g.DataDomainId,
//    g.DevBaseUrl,
//    g.QaBaseUrl,
//    g.PreProdBaseUrl,
//    g.ProdBaseUrl,
//    g.EndpointPath,
//    g.EntityKey,
//    g.Metadata
//FROM DataDomainConfigs AS d
//LEFT JOIN DomainSourceTypeGraphQLs AS g 
//    ON d.Id = g.DataDomainId
//WHERE d.IsActive = @isActive;
