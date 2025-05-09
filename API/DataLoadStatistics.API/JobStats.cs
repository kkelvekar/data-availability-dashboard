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

        public static List<JobStats> GenerateRandomJobStats(DateTime jobStartDate)
        {
            // Generate static data: 5 records per entity, all with RecordAsOfDate == jobStartDate
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

            const int recordsPerEntity = 5;
            foreach (var entity in businessEntities)
            {
                for (int i = 0; i < recordsPerEntity; i++)
                {
                    statsList.Add(new JobStats
                    {
                        Id = Guid.NewGuid(),
                        BusinessEntity = entity,
                        JobStart = jobStartDate,
                        JobEnd = jobStartDate,
                        JobStatus = "Success",
                        RecordAsOfDate = jobStartDate,
                        QualityStatus = "Pass",
                        RecordLoaded = 0,
                        RecordFailed = 0
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
