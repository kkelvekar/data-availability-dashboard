using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain.Entities;
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

        public Task<List<DataDomainConfig>> GetAll(bool isActive)
        {
            throw new NotImplementedException();
        }
    }
}
