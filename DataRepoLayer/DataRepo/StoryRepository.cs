using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using ViewModel;

namespace DataRepoLayer
{
    /// <summary>
    /// Repository for managing story-related operations.
    /// </summary>
    public class StoryRepository: IStoryRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for API requests.</param>
        /// <param name="apiOptions">The API settings configuration.</param>
        public StoryRepository(HttpClient httpClient,
            IOptions<ApiSettings> apiOptions)
        {
            _httpClient = httpClient;
            _apiSettings = apiOptions.Value;

        }

        /// <summary>
        /// This method returns top 500 news Ids
        /// </summary>
        /// <returns>500 Top News Ids</returns>
        public async Task<int[]> FetchTopStoryIdsAsync()
        {
            return (await _httpClient.GetFromJsonAsync<int[]>(_apiSettings.BaseUrl + "/topstories.json")) ?? Array.Empty<int>();
        }

        /// <summary>
        /// This method returns news story details.
        /// </summary>
        /// <param name="storyId">The storyId of the news.</param>
        /// <returns>News details</returns>
        public async Task<StoryModel> FetchStoryDetailsAsync(int storyId)
        {
            return (await _httpClient.GetFromJsonAsync<StoryModel>(_apiSettings.BaseUrl + $"/item/{storyId}.json")) ?? new StoryModel();
        }
    }
}
