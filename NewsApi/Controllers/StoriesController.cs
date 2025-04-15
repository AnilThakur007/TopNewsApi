using Microsoft.AspNetCore.Mvc;
using ServiceLayer;
using ViewModel;

namespace NewsApi.Controllers
{
    /// <summary>
    /// API controller for managing stories.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly ILogger<StoriesController> _logger;
        private readonly IStoriesService _storiesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoriesController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="storiesService">The service for retrieving stories.</param>
        public StoriesController(
            ILogger<StoriesController> logger,
            IStoriesService storiesService)
        {
            _logger = logger;
            _storiesService = storiesService;
        }

        /// <summary>
        /// Retrieves the top stories based on the specified pagination and search criteria.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination. Must be greater than zero.</param>
        /// <param name="pageSize">The number of stories per page. Must be greater than zero.</param>
        /// <param name="searchQuery">The search query to filter stories. Can be null or empty for no filtering.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the top stories that match the specified criteria.
        /// </returns>
        [HttpGet("GetTopStories")]
        public async Task<IActionResult> GetTopStoriesAsync(int pageNumber,int pageSize,string searchQuery = "")
        {
            if(pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number and page size must be greater than 0.");
            }
            var stories = await _storiesService.GetTopStoriesAsync(pageNumber,pageSize,searchQuery);
            return Ok(stories);
        }
    }
}
