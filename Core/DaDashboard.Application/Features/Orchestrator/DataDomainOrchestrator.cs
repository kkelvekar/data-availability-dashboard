using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DaDashboard.Application.Features.Orchestrator
{
    /// <summary>
    /// Orchestrates retrieval and aggregation of business entity summaries by delegating job stats retrieval to appropriate strategies.
    /// </summary>
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly JobStatsStrategyFactory _strategyFactory;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<DataDomainOrchestrator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDomainOrchestrator"/> class.
        /// </summary>
        /// <param name="strategyFactory">Factory to resolve job stats strategies by name.</param>
        /// <param name="businessEntityRepository">Repository for retrieving active business entities.</param>
        /// <param name="logger">Logger instance for diagnostic messages.</param>
        public DataDomainOrchestrator(JobStatsStrategyFactory strategyFactory,
                                      IBusinessEntityRepository businessEntityRepository,
                                      ILogger<DataDomainOrchestrator> logger)
        {
            _strategyFactory = strategyFactory;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
        }


        /// <summary>
        /// Retrieves active business entities from the repository, then consumes the JobStatsService 
        /// to fetch job stats using the dynamically retrieved list of business entity names. The stats are 
        /// grouped by BusinessEntity and summarized.
        /// </summary>
        /// <returns>A list of summarized job stats grouped by business entity.</returns>
        public async Task<IEnumerable<BusinessEntitySummary>> GetBusinessEntitySummaryAsync()
        {
            try
            {
                // Retrieve active business entities and extract their names.
                var activeBusinessEntities = await _businessEntityRepository.GetActiveBusinessEntitiesWithDetailsAsync();
                var allStats = new List<Models.Infrastructure.DataLoadStatistics.JobStats>();
                var businessEntityConfigGroups = activeBusinessEntities.GroupBy(be => be.BusinessEntityConfig);

                foreach (var businessEntityConfigGroup in businessEntityConfigGroups)
                {               
                    var serviceName = businessEntityConfigGroup.Key.Name;

                    // lookup strategy by its Name property
                    var strat = _strategyFactory.GetStrategy(serviceName);

                    // fetch stats for *this* group
                    var stats = await strat.GetJobStatsAsync(businessEntityConfigGroup);
                    allStats.AddRange(stats);
                }
         
                // Group job stats by BusinessEntity and build summary DTOs using a dedicated helper.
                var summary = allStats
                    .GroupBy(js => js.BusinessEntity)
                    .Select(group => BuildBusinessEntitySummary(group.Key, group, activeBusinessEntities))
                    .ToList();

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the JobStats summary.");
                throw;
            }
        }

        /// <summary>
        /// Builds a BusinessEntitySummary for a given business entity by mapping properties
        /// from the corresponding active BusinessEntity and aggregating job statistics.
        /// </summary>
        /// <param name="businessEntityName">The business entity name from the JobStats group key.</param>
        /// <param name="jobStatsGroup">The grouped JobStats for the business entity.</param>
        /// <param name="activeEntities">The collection of active business entities.</param>
        /// <param name="random">A Random instance for generating random selections.</param>
        /// <returns>A populated BusinessEntitySummary object.</returns>
        private BusinessEntitySummary BuildBusinessEntitySummary(
            string businessEntityName,
            IGrouping<string, Models.Infrastructure.DataLoadStatistics.JobStats> jobStatsGroup,
            IEnumerable<BusinessEntity> activeEntities)
        {
            var random = new Random();
            var matchingEntity = GetBusinessEntityDetails(businessEntityName, activeEntities);
            var applicationOwner = matchingEntity?.ApplicationOwner ?? string.Empty;

            // Split the comma-separated string of dependent functionalities.
            var dependentFuncs = matchingEntity != null && !string.IsNullOrWhiteSpace(matchingEntity.DependentFunctionalities)
                ? matchingEntity.DependentFunctionalities
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                : Enumerable.Empty<string>();

            return new BusinessEntitySummary
            {
                Id = Guid.NewGuid(),
                ApplicationOwner = applicationOwner,
                BusinessEntity = businessEntityName,
                LatestLoadDate = jobStatsGroup.Max(js => js.RecordAsOfDate),
                TotalRecordsLoaded = jobStatsGroup.Sum(js => js.RecordLoaded),
                DependentFuncs = dependentFuncs,
                Status = new EntityStatus
                {
                    Indicator = (RagIndicator)random.Next(0, 3),
                    Description = $"Auto-generated status {random.Next(1000, 9999)}"
                }
            };
        }

        /// <summary>
        /// Retrieves the corresponding active BusinessEntity based on the given business entity name.
        /// </summary>
        /// <param name="businessEntityName">The business entity name from the JobStats group key.</param>
        /// <param name="activeEntities">The collection of active BusinessEntity objects from the repository.</param>
        /// <returns>The matching BusinessEntity if found; otherwise, null.</returns>
        private BusinessEntity? GetBusinessEntityDetails(string businessEntityName, IEnumerable<BusinessEntity> activeEntities)
        {
            return activeEntities
                .FirstOrDefault(be => be.Name.Equals(businessEntityName, StringComparison.OrdinalIgnoreCase));
        }

    }
}
