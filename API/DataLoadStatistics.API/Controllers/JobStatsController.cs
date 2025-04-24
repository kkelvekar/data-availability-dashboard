using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using DataLoadStatistics.API;
using DataLoadStatistics.API.DTO;

namespace DataLoadStatistics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobStatsController : ControllerBase
    {
        private readonly ILogger<JobStatsController> _logger;

        public JobStatsController(ILogger<JobStatsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetJobStats([FromQuery] JobStatsFilterRequest request)
        {
            // Determine the reference date (RecordAsOfDate filter); default to today if not provided.
            DateTime selectedDate = request.RecordAsOfDate?.Date ?? DateTime.Today;
            // Generate static job statistics records for the given date.
            var jobStatsList = JobStats.GenerateRandomJobStats(selectedDate);

            JobStats.PrintJobStatsTable(jobStatsList);

            // Filter by business entities if provided.
            if (request.BusinessEntities != null && request.BusinessEntities.Any())
            {
                jobStatsList = jobStatsList
                    .Where(js => request.BusinessEntities.Contains(js.BusinessEntity))
                    .ToList();
            }


            // For each business entity, select the records that have the maximum available RecordAsOfDate.
            // This returns a flat list that contains all rows corresponding to that "latest" date per entity.
            var latestJobStats = jobStatsList
                .GroupBy(js => js.BusinessEntity)
                .SelectMany(group =>
                {
                    // Get the maximum RecordAsOfDate for this business entity as of selectedDate.
                    var maxDate = group.Max(js => js.RecordAsOfDate);
                    // Return all job stats for this business entity that have that date.
                    return group.Where(js => js.RecordAsOfDate == maxDate);
                })
                .ToList();

            return Ok(latestJobStats); ;
        }
    }
}
