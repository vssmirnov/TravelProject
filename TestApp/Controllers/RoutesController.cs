using Microsoft.AspNetCore.Mvc;
using TestApp.CustomException;
using TestApp.Models;
using TestApp.Services;

namespace TestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public RoutesController(ISearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping(CancellationToken cancellationToken)
        {
            var isAvailable = await _searchService.IsAvailableAsync(cancellationToken);
            return isAvailable ? Ok() : StatusCode(500);
        }

        [HttpPost]
        public async Task<ActionResult<SearchResponse>> SearchAsync([FromBody] SearchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _searchService.SearchAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (ServiceUnavailableException)
            {
                return StatusCode(500, "Both providers are unavailable");
            }
        }
    }
}