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

        public static List<JobStats> GenerateRandomJobStats()
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


            Random random = new Random();

            foreach (var entity in businessEntities)
            {
                var entityRecords = new List<JobStats>();

                // ---------------------------------------------------
                // 1. Define the "latest available date" for this entity.
                // Choose a date from today, yesterday or T-2.
                int latestDaysAgo = random.Next(0, 3); // 0, 1, or 2 days ago.
                DateTime latestDate = DateTime.Today.AddDays(-latestDaysAgo);

                // ---------------------------------------------------
                // 2. Force creation of 5 records for the "latest" group.
                for (int i = 0; i < 5; i++)
                {
                    var record = CreateRandomJobStatsRecord(entity, latestDate, random);
                    entityRecords.Add(record);
                }

                // ---------------------------------------------------
                // 3. Force creation of 5 records for the "older" group.
                // They must have a RecordAsOfDate that is strictly older than latestDate.
                int minOlderDays = latestDaysAgo + 1; // Ensure strictly older.
                for (int i = 0; i < 5; i++)
                {
                    // Pick a day in the range [minOlderDays, 29]. (29 means 29 days ago.)
                    int olderDaysAgo = random.Next(minOlderDays, 30);
                    DateTime olderDate = DateTime.Today.AddDays(-olderDaysAgo);
                    var record = CreateRandomJobStatsRecord(entity, olderDate, random);
                    entityRecords.Add(record);
                }

                // ---------------------------------------------------
                // 4. Optionally generate additional random records.
                // Choose a random number between 0 and 10.
                int additionalCount = random.Next(0, 11);
                for (int i = 0; i < additionalCount; i++)
                {
                    // To ensure records are not newer than the designated latestDate, 
                    // choose a random day between latestDaysAgo and 29.
                    int daysAgoAdditional = random.Next(latestDaysAgo, 30);
                    DateTime additionalDate = DateTime.Today.AddDays(-daysAgoAdditional);
                    var record = CreateRandomJobStatsRecord(entity, additionalDate, random);
                    entityRecords.Add(record);
                }

                statsList.AddRange(entityRecords);
            }

            // Shuffle the overall list so that records from different entities are mixed.
            statsList = statsList.OrderBy(x => random.NextDouble()).ToList();
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

        /// <summary>
        /// Helper method to create a random JobStats record for a given entity and record date.
        /// </summary>
        private static JobStats CreateRandomJobStatsRecord(string entity, DateTime recordDate, Random random)
        {
            // Generate a random JobStart time between 9:00 AM and 2:00 PM on the recordDate.
            int hour = random.Next(9, 15); // 9 to 14 (15 is exclusive)
            int minute = random.Next(0, 60);
            DateTime jobStart = new DateTime(recordDate.Year, recordDate.Month, recordDate.Day, hour, minute, 0);

            // Generate JobEnd by adding between 20 and 60 minutes to the JobStart time.
            int duration = random.Next(20, 61);
            DateTime jobEnd = jobStart.AddMinutes(duration);

            // Determine job outcome: 70% chance of success.
            bool isSuccess = random.NextDouble() > 0.3;
            string jobStatus = isSuccess ? "Success" : "Failure";
            string qualityStatus = isSuccess ? "Pass" : "Fail";

            // Set record counts based on job outcome.
            int recordLoaded = isSuccess ? random.Next(100, 300) : 0;
            int recordFailed = isSuccess ? 0 : random.Next(10, 101);

            return new JobStats
            {
                Id = Guid.NewGuid(),
                BusinessEntity = entity,
                JobStart = jobStart,
                JobEnd = jobEnd,
                JobStatus = jobStatus,
                RecordAsOfDate = recordDate,
                QualityStatus = qualityStatus,
                RecordLoaded = recordLoaded,
                RecordFailed = recordFailed
            };
        }
    }
}
