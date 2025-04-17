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
        private readonly ILogger<DataAvailabilityController> _logger;

        public DataAvailabilityController(IDataDomainOrchestrator dataDomainOrchestrator, IMapper mapper, ILogger<DataAvailabilityController> logger)
        {
            _dataDomainOrchestrator = dataDomainOrchestrator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("business-entity-summary")]
        public async Task<IActionResult> GetBusinessEntitySummary()
        {
            try
            {
                var summaries = await _dataDomainOrchestrator.GetBusinessEntitySummaryAsync();
                var response = _mapper.Map<IEnumerable<BusinessEntitySummaryResponse>>(summaries);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve business entity summary");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { Error = "An error occurred while processing your request." }
                );
            }
        }
    }
}
