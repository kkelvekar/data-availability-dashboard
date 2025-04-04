using System;
using System.Collections.Generic;

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
                "Account Static: Portfolios",
                "Account Static: GIM SCD Mapping",
                "Account Static: Strategies",
                "Account Static: Internal Contracts"
            };

            Random random = new Random();

            // Randomly select one or two business entities that should NOT have any records for the current date.
            int numberToExclude = random.Next(1, 3); // will be 1 or 2.
            var excludeEntities = new HashSet<string>();
            while (excludeEntities.Count < numberToExclude)
            {
                string candidate = businessEntities[random.Next(0, businessEntities.Length)];
                excludeEntities.Add(candidate);
            }

            // For each business entity, generate a random number of records between 5 and 10.
            foreach (var entity in businessEntities)
            {
                int recordCount = random.Next(5, 11); // generates between 5 and 10 records.
                for (int i = 0; i < recordCount; i++)
                {
                    DateTime recordAsOfDate;
                    // 70% chance: generate a "recent" record.
                    // For entities in the exclusion list, force daysAgo to be 1 or 2 (T‑1 or T‑2).
                    // For others, allow 0 (current date) as well.
                    if (random.NextDouble() < 0.7)
                    {
                        int daysAgo = excludeEntities.Contains(entity)
                            ? random.Next(1, 3)   // 1 or 2 days ago.
                            : random.Next(0, 3);  // 0, 1, or 2 days ago.
                        recordAsOfDate = DateTime.Today.AddDays(-daysAgo);
                    }
                    else
                    {
                        // 30% chance: choose a random date between T‑7 and T‑3.
                        int daysAgo = random.Next(3, 8); // 3 to 7 days ago.
                        recordAsOfDate = DateTime.Today.AddDays(-daysAgo);
                    }

                    // Generate a random JobStart time on the RecordAsOfDate between 9:00 AM and 2:00 PM.
                    int hour = random.Next(9, 15); // 9 AM up to 2 PM.
                    int minute = random.Next(0, 60);
                    DateTime jobStart = new DateTime(recordAsOfDate.Year, recordAsOfDate.Month, recordAsOfDate.Day, hour, minute, 0);

                    // Set JobEnd between 20 to 60 minutes after JobStart.
                    int duration = random.Next(20, 61);
                    DateTime jobEnd = jobStart.AddMinutes(duration);

                    // Randomly decide if the job is a success (70% chance) or failure.
                    bool isSuccess = random.NextDouble() > 0.3;
                    string jobStatus = isSuccess ? "Success" : "Failure";
                    string qualityStatus = isSuccess ? "Pass" : "Fail";

                    // Set record counts based on job status.
                    int recordLoaded = isSuccess ? random.Next(100, 300) : 0;
                    int recordFailed = isSuccess ? 0 : random.Next(10, 101);

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
    }
}
