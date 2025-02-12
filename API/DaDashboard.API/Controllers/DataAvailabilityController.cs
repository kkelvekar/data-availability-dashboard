using DaDashboard.API.DTO;
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
        public async Task<IActionResult> GetDataDomains(DateTime? date)
        {
            var dataDomains = await _dataDomainOrchestrator.GetDataDomainsAsync(date);
            var responseList = dataDomains.SelectMany(domain =>
            domain.Metrics.Select(metric => new DataDomainResponse
                                           {
                                               Id = domain.Id,
                                               DomainName = domain.Name,
                                               BusinessEntity = metric.EntityKey, // Map each metric's EntityKey
                                               Count = metric.Count,
                                               LoadDate = metric.Date
                                           })).ToList();
            return Ok(dataDomains);
        }
    }
}
