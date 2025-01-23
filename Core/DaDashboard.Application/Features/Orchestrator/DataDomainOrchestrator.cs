using DaDashboard.Application.Contracts.Application;
using DaDashboard.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        public IEnumerable<DataDomain> GetDataDomains(string date)
        {
            return new List<DataDomain>
            {
                new DataDomain
                {
                    Name = "DataDomain1",
                    LoadDate = DateTime.Now,
                    Count = 1
                },
                new DataDomain
                {
                    Name = "DataDomain2",
                    LoadDate = DateTime.Now,
                    Count = 2
                },
                new DataDomain
                {
                    Name = "DataDomain3",
                    LoadDate = DateTime.Now,
                    Count = 3
                }
            };
        }
    }
}
