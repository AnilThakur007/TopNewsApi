using DataRepoLayer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ViewModel;

namespace ServiceLayer
{
    /// <summary>
    /// Service implementation for retrieving stories.
    /// </summary>
    public class StoriesService: IStoriesService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ApiSettings _apiSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoriesService"/> class.
        /// </summary>
        /// <param name="storyRepository">The repository for retrieving story data.</param>
        /// <param name="memoryCache">The memory cache for storing frequently accessed data.</param>
        /// <param name="cacheOptions">The cache settings configuration.</param>
        /// <param name="apiOptions">The API settings configuration.</param>
        public StoriesService(IStoryRepository storyRepository,
            IMemoryCache memoryCache,
            IOptions<CacheSettings> cacheOptions,
            IOptions<ApiSettings> apiOptions)
        {
            _storyRepository = storyRepository;
            _memoryCache = memoryCache;
            _cacheSettings = cacheOptions.Value;
            _apiSettings = apiOptions.Value;
        }

        /// <summary>
        /// Retrieves a paginated and filtered collection of top stories based on the given parameters.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination. Must be greater than zero.</param>
        /// <param name="pageSize">The number of stories per page. Must be greater than zero.</param>
        /// <param name="searchQuery">The search query to filter the stories. Can be null or empty for no filtering.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a collection of
        /// <see cref="StoryModel"/> instances that match the specified criteria.
        /// </returns>
        /// <remarks>
        /// If no top stories are available, an empty collection is returned.
        /// </remarks>
        public async Task<IEnumerable<StoryModel>> GetTopStoriesAsync(int pageNumber, int pageSize, string searchQuery)
        {
            var topStoryIds = await GetTopStoryIdsAsync();
            if (topStoryIds?.Length > 0)
            {
                var stories = await GetStoriesAsync(topStoryIds.Take(_apiSettings.MaxStories));
                return FilterAndPaginateStories(stories, pageNumber, pageSize, searchQuery);
            }
            return Enumerable.Empty<StoryModel>();
        }

        /// <summary>
        /// Retrieves a paginated and filtered collection of top stories based on the given parameters.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a collection of
        /// <see cref="StoryModel"/> instances that match the specified criteria.
        /// </returns>
        /// <remarks>
        /// If no top stories are available, an empty collection is returned.
        /// </remarks>

        public async Task<int[]> GetTopStoryIdsAsync()
        {
            var topStoryIds = await _memoryCache.GetOrCreateAsync("Top200Stories", async entry =>
            {
                ConfigureCacheEntry(entry);
                return await _storyRepository.FetchTopStoryIdsAsync() ?? Array.Empty<int>();
            });

            return topStoryIds ?? Array.Empty<int>();
        }

        private void ConfigureCacheEntry(ICacheEntry entry)
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.AbsoluteExpirationMinutes);
            entry.SlidingExpiration = TimeSpan.FromMinutes(_cacheSettings.SlidingExpirationMinutes);
        }

        private async Task<List<StoryModel>> GetStoriesAsync(IEnumerable<int> storyIds)
        {
            var storyTasks = storyIds.Select(id =>
                _memoryCache.GetOrCreateAsync($"Story_{id}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.AbsoluteExpirationMinutes);
                    return await _storyRepository.FetchStoryDetailsAsync(id);
                })
            );
            var stories = await Task.WhenAll(storyTasks);
            return stories.Where(story => story != null).Select(story => story!).ToList();
        }

        private IEnumerable<StoryModel> FilterAndPaginateStories(IEnumerable<StoryModel> stories, int pageNumber, int pageSize, string searchQuery)
        {
            var filteredStories = stories.Where(story =>
            {
                // If the search query is empty or null
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    return true; // Include all stories
                }
                // If the story title contains the search query (case-insensitive)
                else if (story.Title?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true; // Include the matching story
                }
                else
                {
                    return false; // Exclude the story
                }
            });

            var paginatedStories = filteredStories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize); 

            return paginatedStories;
        }

    }
}
