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
                var stories = await GetStoriesAsync(topStoryIds);
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
            var topStoryIds = await _memoryCache.GetOrCreateAsync("Top500Stories", async entry =>
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

        //private async Task<List<StoryModel>> GetStoriesAsync(IEnumerable<int> storyIds)
        //{
        //    var storyTasks = storyIds.Select(id =>
        //        _memoryCache.GetOrCreateAsync($"Story_{id}", async entry =>
        //        {
        //            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.AbsoluteExpirationMinutes);
        //            return await _storyRepository.FetchStoryDetailsAsync(id);
        //        })
        //    );
        //    var stories = await Task.WhenAll(storyTasks);
        //    return stories.Where(story => story != null).Select(story => story!).ToList();
        //}

        private async Task<List<StoryModel>> GetStoriesAsync(IEnumerable<int> storyIds)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.AbsoluteExpirationMinutes)
            };

            var validStories = new List<StoryModel>();
            var fetchTasks = new List<Task<StoryModel>>();

            foreach (var id in storyIds.OrderByDescending(id => id))
            {
                if (validStories.Count >= _apiSettings.MaxStories)
                {
                    break; // Stop fetching when max limit is reached
                }

                if (_memoryCache.TryGetValue($"Story_{id}", out StoryModel? cachedStory))
                {
                    if (!string.IsNullOrWhiteSpace(cachedStory?.Url))
                    {
                        validStories.Add(cachedStory);
                    }
                    continue;
                }

                fetchTasks.Add(Task.Run(async () =>
                {
                    var story = await _storyRepository.FetchStoryDetailsAsync(id);
                    _memoryCache.Set($"Story_{id}", story, cacheOptions); // Cache every record

                    return story; // Return the story regardless of URL validity
                }));
            }

            var fetchedStories = await Task.WhenAll(fetchTasks);

            validStories.AddRange(fetchedStories.Where(story => !string.IsNullOrWhiteSpace(story?.Url)).Select(story => story!));

            return validStories.Take(_apiSettings.MaxStories).ToList();
        }

        private IEnumerable<StoryModel> FilterAndPaginateStories(IEnumerable<StoryModel> stories, int pageNumber, int pageSize, string searchQuery)
        {
            var filteredStories = stories
            .Where(story => string.IsNullOrWhiteSpace(searchQuery) || (story.Title?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

            // udpate total records
            filteredStories = filteredStories.Select(story =>
            {
                story.TotalRecords = filteredStories.Count();
                return story; // Ensure you return the modified story object.
            }).ToList();

            var paginatedStories = filteredStories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize); 

            return paginatedStories;
        }

    }
}
