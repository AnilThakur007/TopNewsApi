using ViewModel;

namespace DataRepoLayer
{
    /// <summary>
    /// Represents a repository for fetching story-related data.
    /// </summary>
    public interface IStoryRepository
    {
        /// <summary>
        /// Fetches the IDs of the top stories asynchronously.
        /// </summary>
        /// <returns>An array of story IDs.</returns>
        Task<int[]> FetchTopStoryIdsAsync();

        /// <summary>
        /// Fetches the details of a specific story asynchronously.
        /// </summary>
        /// <param name="storyId">The ID of the story to retrieve.</param>
        /// <returns>A <see cref="StoryModel"/> containing the details of the story.</returns>
        Task<StoryModel> FetchStoryDetailsAsync(int storyId);
    }
}
