using DaDashboard.Application.Contracts.Application;
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
        public IActionResult GetDataDomains(string date)
        {
            var dataDomains = _dataDomainOrchestrator.GetDataDomains(date);
            return Ok(dataDomains);
        }
    }
}
