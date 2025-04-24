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

            var rng = new Random();

            foreach (var entity in businessEntities)
            {
                // Determine how many records to generate (5 to 20).
                int recordCount = rng.Next(5, 21);
                // Ensure at least 3-4 records use the provided date
                int minProvidedCount = rng.Next(5, 10);

                for (int j = 0; j < recordCount; j++)
                {
                    // Decide RecordAsOfDate: guarantee the first minProvidedCount use provided date
                    DateTime asOfDate;
                    if (j < minProvidedCount)
                    {
                        asOfDate = recordAsOfDate;
                    }
                    else
                    {
                        // 50% chance to use provided date thereafter
                        if (rng.NextDouble() < 0.5)
                        {
                            asOfDate = recordAsOfDate;
                        }
                        else
                        {
                            // Older date: 1 to 6 months back
                            int monthsBack = rng.Next(1, 7);
                            var targetMonth = recordAsOfDate.AddMonths(-monthsBack);
                            int daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
                            int day = rng.Next(1, daysInMonth + 1);
                            asOfDate = new DateTime(targetMonth.Year, targetMonth.Month, day);
                        }
                    }

                    // Schedule job start times between 00:00 and 23:30
                    int hour = rng.Next(0, 24);
                    int minute = rng.Next(0, 2) * 30; // 0 or 30
                    DateTime jobStart = new DateTime(asOfDate.Year, asOfDate.Month, asOfDate.Day, hour, minute, 0);
                    DateTime jobEnd = jobStart.AddMinutes(rng.Next(15, 61)); // Duration 15 to 60 mins

                    bool isSuccess = rng.NextDouble() < 0.8; // 80% success rate
                    string jobStatus = isSuccess ? "Success" : "Failure";
                    string qualityStatus = isSuccess ? "Pass" : "Fail";

                    // Random loaded count between 10 and 3000; if failure, loaded = 0
                    int recordLoaded = isSuccess ? rng.Next(10, 3001) : 0;
                    int recordFailed = isSuccess ? 0 : rng.Next(1, 101);

                    statsList.Add(new JobStats
                    {
                        Id = Guid.NewGuid(),
                        BusinessEntity = entity,
                        JobStart = jobStart,
                        JobEnd = jobEnd,
                        JobStatus = jobStatus,
                        RecordAsOfDate = asOfDate,
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
