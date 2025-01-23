using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Contracts.Persistence
{
    public interface IDataDomainConfigRepository : IAsyncRepository<DataDomainConfig>
    {
        Task<List<DataDomainConfig>> GetAll(bool isActive);
    }
}
