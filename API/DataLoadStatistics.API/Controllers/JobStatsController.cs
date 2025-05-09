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
            // Determine the reference date (ReferenceDate filter); default to today if not provided.
            DateTime referenceDate = request.ReferenceDate?.Date ?? DateTime.Today;
            // Generate static job statistics records for the given date.
            var jobStatsList = JobStats.GenerateRandomJobStats(referenceDate);

            // JobStats.PrintJobStatsTable(jobStatsList);

            // Filter by business entities if provided.
            if (request.BusinessEntities != null && request.BusinessEntities.Any())
            {
                jobStatsList = jobStatsList
                    .Where(js => request.BusinessEntities.Contains(js.BusinessEntity))
                    .ToList();
            }

            var latestJobStats = jobStatsList
                .GroupBy(js => js.BusinessEntity)
                .SelectMany(g => {
                    var md = g.Max(x => x.JobStart.Date);
                    return g.Where(x => x.JobStart.Date == md);
                })
                .ToList();

            return Ok(latestJobStats); ;
        }
    }
}
