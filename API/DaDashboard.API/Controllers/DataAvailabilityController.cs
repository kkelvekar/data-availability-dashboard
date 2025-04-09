using AutoMapper;
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
        private readonly IMapper _mapper;

        public DataAvailabilityController(IDataDomainOrchestrator dataDomainOrchestrator, IMapper mapper)
        {
            _dataDomainOrchestrator = dataDomainOrchestrator;
            _mapper = mapper;
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

        // New endpoint to call the GetBusinessEntitySummaryAsync method.
        [HttpGet("business-entity-summary")]
        public async Task<IActionResult> GetBusinessEntitySummary()
        {
            // Retrieve the domain model results.
            var domainSummaries = await _dataDomainOrchestrator.GetBusinessEntitySummaryAsync();

            // Use AutoMapper to map the domain models to the API DTO.
            var response = _mapper.Map<IEnumerable<BusinessEntitySummaryResponse>>(domainSummaries);

            return Ok(response);
        }
    }
}
