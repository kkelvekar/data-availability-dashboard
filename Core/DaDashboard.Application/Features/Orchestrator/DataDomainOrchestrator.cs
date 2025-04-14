using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Domain;
using Microsoft.Extensions.Logging;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class DataDomainOrchestrator : IDataDomainOrchestrator
    {
        private readonly IJobStatsService _jobStatsService;
        private readonly ILogger<DataDomainOrchestrator> _logger;

        public DataDomainOrchestrator(
            IJobStatsService jobStatsService,
            ILogger<DataDomainOrchestrator> logger)
        {
            _jobStatsService = jobStatsService;
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
                // Define a static list of business entities.
                var staticBusinessEntities = new List<string>
                {
                    "Account Static: Bank Accounts",
                    "Account Static: GIN SOO Mapping",
                    "Account Static: Internal Contacts",
                    "Account Static: Portfolios",
                    "Account Static: Strategies",
                    "Benchmark",
                    "Benchmarks: Composition",
                    "Benchmarks: Weight Allocation",
                    "FI Analytics: Benchmark Holding",
                    "FI Analytics: Portfolio Holding",
                    "FX Rate",
                    "Securities",
                    "Security Pricing",
                    "Transactions"
                };

                // Create the infrastructure-layer JobStatsRequest.
                var infraRequest = new Models.Infrastructure.DataLoadStatistics.JobStatsRequest
                {
                    BusinessEntities = staticBusinessEntities,
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
                        DependentFuncs = new List<string>
                        {
                            "Portal", "Optimizer", "Currency",
                            "Portfolio Services", "Strategy Manager"
                        }.OrderBy(x => random.Next()).Take(random.Next(2, 5)),
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

        //private void PrintJobStatsTable(List<Models.Infrastructure.DataLoadStatistics.JobStats> jobStats)
        //{
        //    // Print header row in yellow.
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("{0,-40} {1,-12} {2,-8} {3,-8} {4,-10} {5,-12} {6,-12} {7,-10}",
        //        "Business Entity", "Record As Of", "Job Start", "Job End", "Job Status", "Loaded", "Failed", "Quality");
        //    Console.ResetColor();

        //    // Print each job stats record.
        //    foreach (var stats in jobStats)
        //    {
        //        // Color green if job was successful, else red.
        //        if (stats.JobStatus.Equals("Success", StringComparison.OrdinalIgnoreCase))
        //        {
        //            Console.ForegroundColor = ConsoleColor.Green;
        //        }
        //        else
        //        {
        //            Console.ForegroundColor = ConsoleColor.Red;
        //        }

        //        // Print each record using formatted output.
        //        Console.WriteLine("{0,-40} {1,-12:yyyy-MM-dd} {2,-8:HH:mm} {3,-8:HH:mm} {4,-10} {5,-12} {6,-12} {7,-10}",
        //            stats.BusinessEntity,
        //            stats.RecordAsOfDate,
        //            stats.JobStart,
        //            stats.JobEnd,
        //            stats.JobStatus,
        //            stats.RecordLoaded,
        //            stats.RecordFailed,
        //            stats.QualityStatus);

        //        Console.ResetColor();
        //    }

        //    // Extra line for readability.
        //    Console.WriteLine();
        //}


    }
}
