using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataLoadStatistics.API
{
    public class JobStats
    {
        public Guid Id { get; set; }
        public string BusinessEntity { get; set; }
        public DateTime JobStart { get; set; }
        public DateTime JobEnd { get; set; }
        public string JobStatus { get; set; }
        public DateTime RecordAsOfDate { get; set; }
        public string QualityStatus { get; set; }
        public int RecordLoaded { get; set; }
        public int RecordFailed { get; set; }

        /// <summary>
        /// Generates a static list of JobStats records for each business entity,
        /// assigning the provided date as the RecordAsOfDate. Each entity has exactly 5 records.
        /// </summary>
        /// <param name="recordAsOfDate">The date to assign to each record's RecordAsOfDate.</param>
        public static List<JobStats> GenerateRandomJobStats(DateTime recordAsOfDate)
        {
            var statsList = new List<JobStats>();

            // Define the business entities.
            string[] businessEntities = new string[]
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

            // For each entity, generate 5 deterministic records
            for (int i = 0; i < businessEntities.Length; i++)
            {
                var entity = businessEntities[i];
                for (int j = 0; j < 5; j++)
                {
                    // Schedule job start times at 9:00, 10:00, ..., 13:00
                    int hour = 9 + j;
                    DateTime jobStart = new DateTime(recordAsOfDate.Year, recordAsOfDate.Month, recordAsOfDate.Day, hour, 0, 0);
                    DateTime jobEnd = jobStart.AddMinutes(30);

                    bool isSuccess = (j % 2 == 0);
                    string jobStatus = isSuccess ? "Success" : "Failure";
                    string qualityStatus = isSuccess ? "Pass" : "Fail";
                    // Vary the loaded record count per entity to ensure different totals
                    int recordLoaded = isSuccess ? (100 + j * 10 + i * 10) : 0;
                    int recordFailed = isSuccess ? 0 : (10 + j * 2);

                    statsList.Add(new JobStats
                    {
                        Id = Guid.NewGuid(),
                        BusinessEntity = entity,
                        JobStart = jobStart,
                        JobEnd = jobEnd,
                        JobStatus = jobStatus,
                        RecordAsOfDate = recordAsOfDate,
                        QualityStatus = qualityStatus,
                        RecordLoaded = recordLoaded,
                        RecordFailed = recordFailed
                    });
                }
            }

            return statsList;
        }

        /// <summary>
        /// Prints a colorized table of job stats to the console.
        /// Headers are printed in yellow.
        /// Records with a JobStatus of "Success" are printed in green; others in red.
        /// </summary>
        /// <param name="jobStats">The list of job stats records to print.</param>
        public static void PrintJobStatsTable(List<JobStats> jobStats)
        {
            // Print header row in yellow.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0,-40} {1,-12} {2,-8} {3,-8} {4,-10} {5,-12} {6,-12} {7,-10}",
                "Business Entity", "Record As Of", "Job Start", "Job End", "Job Status", "Loaded", "Failed", "Quality");
            Console.ResetColor();

            // Print each job stat record.
            foreach (var stats in jobStats)
            {
                // Set text color based on JobStatus.
                if (stats.JobStatus.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                // Print the record with formatted columns.
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

            // Add an extra blank line for clarity.
            Console.WriteLine();
        }

    }
}
