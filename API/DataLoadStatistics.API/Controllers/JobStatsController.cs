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

            // Filter by RecordAsOfDate if provided.
            if (request.RecordAsOfDate.HasValue)
            {
                jobStatsList = jobStatsList
                    .Where(js => js.RecordAsOfDate.Date == request.RecordAsOfDate.Value.Date)
                    .ToList();
            }

            return Ok(jobStatsList);
        }
    }
}
