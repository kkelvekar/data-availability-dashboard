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
                // How many records to generate (5 to 20).
                int recordCount = rng.Next(5, 21);
                int minProvidedCount = rng.Next(5, 10); // at least this many on the provided date

                for (int j = 0; j < recordCount; j++)
                {
                    // 1) Pick the calendar date for this job run
                    DateTime runDate;
                    if (j < minProvidedCount || rng.NextDouble() < 0.5)
                    {
                        // guaranteed or 50% chance: exactly the provided date
                        runDate = jobStartDate.Date;
                    }
                    else
                    {
                        // otherwise pick an older date 1–6 months back, with a random day
                        int monthsBack = rng.Next(1, 7);
                        var targetMonth = jobStartDate.AddMonths(-monthsBack);
                        int daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
                        int day = rng.Next(1, daysInMonth + 1);
                        runDate = new DateTime(targetMonth.Year, targetMonth.Month, day);
                    }

                    // 2) Schedule job start/end times on runDate
                    int hour = rng.Next(0, 24);
                    int minute = rng.Next(0, 2) * 30; // 0 or 30
                    DateTime jobStart = runDate.AddHours(hour).AddMinutes(minute);
                    DateTime jobEnd = jobStart.AddMinutes(rng.Next(15, 61)); // 15–60 mins

                    // 3) Determine success/failure
                    bool isSuccess = rng.NextDouble() < 0.8; // 80% success
                    string jobStatus = isSuccess ? "Success" : "Failure";
                    string qualityStatus = isSuccess ? "Pass" : "Fail";
                    int recordLoaded = isSuccess ? rng.Next(10, 3001) : 0;
                    int recordFailed = isSuccess ? 0 : rng.Next(1, 101);

                    // 4) Randomize RecordAsOfDate to be job start date, or 1 or 2 days before
                    int offsetDays = rng.Next(0, 3); // 0, 1 or 2
                    DateTime recordAsOfDate = jobStart.Date.AddDays(-offsetDays);

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
