using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly IDataDomainConfigRepository _dataDomainConfigRepository;
        private readonly IEnumerable<IDataSourceService> _dataSourceServices;
        private readonly IJobStatsService _jobStatsService;
        private readonly ILogger<DataDomainOrchestrator> _logger;

        public DataDomainOrchestrator(
            IDataDomainConfigRepository dataDomainConfigRepository,
            IEnumerable<IDataSourceService> dataSourceServices,
            IJobStatsService jobStatsService,
            ILogger<DataDomainOrchestrator> logger)
        {
            _dataDomainConfigRepository = dataDomainConfigRepository;
            _dataSourceServices = dataSourceServices;
            _jobStatsService = jobStatsService;
            _logger = logger;
        }

        public async Task<IEnumerable<DataDomain>?> GetDataDomainsAsync(DateTime? effectiveDate)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data domains.");
                return null;
            }
        }

        /// <summary>
        /// Calls the appropriate data source service for a given domain config.
        /// </summary>
        private async Task<DataDomain> GetDataDomainForConfigAsync(DataDomainConfig config, IDataSourceService service, DateTime? effectiveDate)
        {
            List<DataMetric> metrics = await service.GetDataMetricAsync(config, effectiveDate);

            return new DataDomain
            {
                Id = config.Id,
                Name = config.DomainName,
                Metrics = metrics
            };
        }

        /// <summary>
        /// Consumes the JobStatsService to fetch job stats using a static list of business entities
        /// and a RecordAsOfDate set to T-2 (or T-3), then groups them by BusinessEntity and returns a summary.
        /// </summary>
        /// <returns>A list of summarized job stats grouped by business entity.</returns>
        public async Task<IEnumerable<BusinessEntitySummary>> GetBusinessEntitySummaryAsync()
        {
            try
            {
                // Define a static list of business entities.
                var staticBusinessEntities = new List<string>
                {
                    "Account Static: Portfolios",
                    "Account Static: GIM SCD Mapping",
                    "Account Static: Strategies",
                    "Account Static: Internal Contracts"
                };

                // Set RecordAsOfDate to T-2 (or use AddDays(-3) for T-3).
                var recordAsOfDate = DateTime.Today.AddDays(-2);

                // Create the infrastructure-layer JobStatsRequest.
                var infraRequest = new Models.Infrastructure.DataLoadStatistics.JobStatsRequest
                {
                    BusinessEntities = staticBusinessEntities,
                    RecordAsOfDate = recordAsOfDate
                };

                // Retrieve job stats from the infrastructure service.
                var jobStats = await _jobStatsService.GetJobStatsAsync(infraRequest);

                // Group job stats by BusinessEntity and build summary DTOs.
                var summary = jobStats
                    .GroupBy(js => js.BusinessEntity)
                    .Select(g => new BusinessEntitySummary
                    {
                        BusinessEntityID = Guid.NewGuid(), // Random GUID for now.
                        ApplicationOwner = "Data Services",
                        BusinessEntity = g.Key,
                        LatestLoadDate = g.Max(js => js.RecordAsOfDate),
                        TotalRecordsLoaded = g.Sum(js => js.RecordLoaded)
                    })
                    .ToList();

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the JobStats summary.");
                return Enumerable.Empty<BusinessEntitySummary>();
            }
        }
    }
}
