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

                // Create the infrastructure-layer JobStatsRequest.
                var infraRequest = new Models.Infrastructure.DataLoadStatistics.JobStatsRequest
                {
                    BusinessEntities = staticBusinessEntities,
                };

                // Retrieve job stats from the infrastructure service.
                var jobStats = await _jobStatsService.GetJobStatsAsync(infraRequest);
              
                // Print the job stats in a table format with color.
                PrintJobStatsTable(jobStats);
                
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

        private void PrintJobStatsTable(List<Models.Infrastructure.DataLoadStatistics.JobStats> jobStats)
        {
            // Print header row in yellow.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0,-40} {1,-12} {2,-8} {3,-8} {4,-10} {5,-12} {6,-12} {7,-10}",
                "Business Entity", "Record As Of", "Job Start", "Job End", "Job Status", "Loaded", "Failed", "Quality");
            Console.ResetColor();

            // Print each job stats record.
            foreach (var stats in jobStats)
            {
                // Color green if job was successful, else red.
                if (stats.JobStatus.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                // Print each record using formatted output.
                Console.WriteLine("{0,-40} {1,-12:yyyy-MM-dd} {2,-8:HH:mm} {3,-8:HH:mm} {4,-10} {5,-12} {6,-12} {7,-10}",
                    stats.BusinessEntity,
                    stats.RecordAsOfDate,
                    stats.JobStart,
                    stats.JobEnd,
                    stats.JobStatus,
                    stats.RecordLoaded,
                    stats.RecordFailed,
                    stats.QualityStatus);

                Console.ResetColor();
            }

            // Extra line for readability.
            Console.WriteLine();
        }


    }
}
