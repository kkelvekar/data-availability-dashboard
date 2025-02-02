﻿using DaDashboard.Application.Contracts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DaDashboard.API.Controllers
{
    [Route("api/data-availability")]
    [ApiController]
    public class DataAvailabilityController : ControllerBase
    {
        private readonly IDataDomainOrchestrator _dataDomainOrchestrator;
        public DataAvailabilityController(IDataDomainOrchestrator dataDomainOrchestrator)
        {
            _dataDomainOrchestrator = dataDomainOrchestrator;
        }

        [HttpGet("data-domain/{date}")]
        public async Task<IActionResult> GetDataDomains(string date)
        {
            var dataDomains = await _dataDomainOrchestrator.GetDataDomainsAsync(date);
            return Ok(dataDomains);
        }
    }
}
