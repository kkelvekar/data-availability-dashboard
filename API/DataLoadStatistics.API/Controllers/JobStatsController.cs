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
            // Generate random job statistics records.
            var jobStatsList = JobStats.GenerateRandomJobStats();

            // Filter by business entities if provided.
            if (request.BusinessEntities != null && request.BusinessEntities.Any())
            {
                jobStatsList = jobStatsList
                    .Where(js => request.BusinessEntities.Contains(js.BusinessEntity))
                    .ToList();
            }

            // Filter records by date range: from the provided RecordAsOfDate (lower bound)
            // up to the current date (upper bound), comparing only the date portion.
            if (request.RecordAsOfDate.HasValue)
            {
                DateTime lowerBound = request.RecordAsOfDate.Value.Date;
                DateTime upperBound = DateTime.Today;
                jobStatsList = jobStatsList
                    .Where(js => js.RecordAsOfDate.Date >= lowerBound && js.RecordAsOfDate.Date <= upperBound)
                    .ToList();
            }

            return Ok(jobStatsList);
        }
    }
}
