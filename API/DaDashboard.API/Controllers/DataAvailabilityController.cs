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
