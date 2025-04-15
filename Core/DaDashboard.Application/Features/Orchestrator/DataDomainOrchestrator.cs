using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly IJobStatsService _jobStatsService;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<DataDomainOrchestrator> _logger;

        public DataDomainOrchestrator(
            IJobStatsService jobStatsService, IBusinessEntityRepository businessEntityRepository, ILogger<DataDomainOrchestrator> logger)
        {
            _jobStatsService = jobStatsService;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
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
                // Retrieve active business entities from the repository.
                var activeBusinessEntities = await _businessEntityRepository.GetActiveBusinessEntitiesWithDetailsAsync();
                var businessEntityNames = activeBusinessEntities.Select(be => be.Name).ToList();

                // Create the infrastructure-layer JobStatsRequest.
                var infraRequest = new Models.Infrastructure.DataLoadStatistics.JobStatsRequest
                {
                    BusinessEntities = businessEntityNames
                };

                // Retrieve job stats from the infrastructure service.
                var jobStats = await _jobStatsService.GetJobStatsAsync(infraRequest);

                // Print the job stats in a table format with color.
                // PrintJobStatsTable(jobStats);

                var random = new Random();
                // Group job stats by BusinessEntity and build summary DTOs.
                var summary = jobStats
                    .GroupBy(js => js.BusinessEntity)
                    .Select(g => new BusinessEntitySummary
                    {
                        Id = Guid.NewGuid(), // Random GUID for now.
                        ApplicationOwner = "Data Services",
                        BusinessEntity = g.Key,
                        LatestLoadDate = g.Max(js => js.RecordAsOfDate),
                        TotalRecordsLoaded = g.Sum(js => js.RecordLoaded),
                        DependentFuncs = MapDependentFunctions(g.Key, activeBusinessEntities),
                        Status = new EntityStatus
                        {
                            Indicator = (RagIndicator)random.Next(0, 3),
                            Description = $"Auto-generated status {random.Next(1000, 9999)}"
                        }
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

        /// <summary>
        /// Maps the DependentFuncs list by using the business entity's name from JobStats to look up the corresponding
        /// active business entity from the repository. It then converts its DependentFunctionalities string into a list of strings.
        /// </summary>
        /// <param name="businessEntityName">The business entity name from the job stats group key.</param>
        /// <param name="activeEntities">The collection of active business entities from the repository.</param>
        /// <returns>An enumerable list of dependent functionality names.</returns>
        private IEnumerable<string> MapDependentFunctions(string businessEntityName, IEnumerable<BusinessEntity> activeEntities)
        {
            // Find the matching active business entity by comparing its Name property with the business entity name from the stats.
            var matchingEntity = activeEntities
                .FirstOrDefault(be => be.Name.Equals(businessEntityName, StringComparison.OrdinalIgnoreCase));

            if (matchingEntity != null && !string.IsNullOrWhiteSpace(matchingEntity.DependentFunctionalities))
            {
                // Split the comma-separated values and trim whitespace.
                return matchingEntity.DependentFunctionalities
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
            }
            // Return an empty list if no match is found or no dependent functionalities are provided.
            return new List<string>();
        }

    }
}
