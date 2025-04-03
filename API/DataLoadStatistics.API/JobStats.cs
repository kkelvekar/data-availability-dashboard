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

            // For each business entity, generate 4 random records.
            foreach (var entity in businessEntities)
            {
                int recordCount = 4; // At least 4 records per business entity.
                for (int i = 0; i < recordCount; i++)
                {
                    // Generate a random JobStart on March 13, 2025 between 9:00 AM and 2:00 PM.
                    int hour = random.Next(9, 15); // 9 AM up to 2 PM.
                    int minute = random.Next(0, 60);
                    DateTime jobStart = new DateTime(2025, 03, 13, hour, minute, 0);

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

                    // For simplicity, use the date part of JobStart as RecordAsOfDate.
                    DateTime recordAsOfDate = jobStart.Date;

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
