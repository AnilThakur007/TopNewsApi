using ViewModel;

namespace ServiceLayer
{
    /// <summary>
    /// Defines a service for retrieving stories.
    /// </summary>
    public interface IStoriesService
    {
        /// <summary>
        /// Retrieves the top stories based on the specified pagination and search criteria.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination. Must be a positive integer.</param>
        /// <param name="pageSize">The number of stories per page. Must be greater than zero.</param>
        /// <param name="searchQuery">The search query to filter stories. Can be null or empty for no filtering.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of <see cref="StoryModel"/>.</returns>
        Task<IEnumerable<StoryModel>> GetTopStoriesAsync(int pageNumber, int pageSize, string searchQuery);
    }
}
