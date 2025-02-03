using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly IDataDomainConfigRepository _dataDomainConfigRepository;
        private readonly IEnumerable<IDataSourceService> _dataSourceServices;

        public DataDomainOrchestrator(IDataDomainConfigRepository dataDomainConfigRepository, IEnumerable<IDataSourceService> dataSourceServices)
        {
            _dataDomainConfigRepository = dataDomainConfigRepository;
            _dataSourceServices = dataSourceServices;
        }

        public async Task<IEnumerable<DataDomain>> GetDataDomainsAsync(DateTime? effectiveDate)
        {       
            // Retrieve all active domain configurations from the DB
            var configs = await _dataDomainConfigRepository.GetAll(isActive: true);

            // Build a dictionary for quick lookup of the service by SourceType (case-insensitive)
            var serviceMap = _dataSourceServices.ToDictionary(s => s.SourceType, StringComparer.OrdinalIgnoreCase);

            // Launch all data source calls concurrently
            var tasks = new List<Task<DataDomain>>();
            foreach (var config in configs)
            {
                if (serviceMap.TryGetValue(config.SourceType, out var service))
                {
                    tasks.Add(GetDataDomainForConfigAsync(config, service, effectiveDate));
                }
            }

            // Await all tasks concurrently and return the aggregated results
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Calls the appropriate data source service for a given domain config.
        /// </summary>
        private async Task<DataDomain> GetDataDomainForConfigAsync(DataDomainConfig config, IDataSourceService service, DateTime? effectiveDate)
        {
            DataMetric metric = await service.GetDataMetricAsync(config, effectiveDate);

            return new DataDomain
            {
                Name = config.DomainName,
                Metric = metric
            };
        }
    }
}
