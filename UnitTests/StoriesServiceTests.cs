using DataRepoLayer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using ServiceLayer;
using ViewModel;

namespace UnitTests
{
    public class StoriesServiceTests
    {
        private readonly Mock<IStoryRepository> _storyRepositoryMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly StoriesService _service;

        public StoriesServiceTests()
        {
            _storyRepositoryMock = new Mock<IStoryRepository>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            var cacheSettings = Options.Create(new CacheSettings { AbsoluteExpirationMinutes = 30, SlidingExpirationMinutes = 15 });
            var apiSettings = Options.Create(new ApiSettings { MaxStories = 200 });

            _service = new StoriesService(_storyRepositoryMock.Object, _memoryCacheMock.Object, cacheSettings, apiSettings);
        }

        [Fact]
        public async Task GetTopStoriesAsync_ReturnsEmpty_WhenNoTopStories()
        {
            // Arrange
            _storyRepositoryMock.Setup(repo => repo.FetchTopStoryIdsAsync())
                .ReturnsAsync(Array.Empty<int>());

            object? cacheValue = null;
            _memoryCacheMock.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out cacheValue))
                           .Returns(false);

            _memoryCacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetTopStoriesAsync(1, 10, "");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _storyRepositoryMock.Verify(repo => repo.FetchTopStoryIdsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTopStoryIdsAsync_CachesTopStories()
        {
            // Arrange
            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(cache => cache.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            _storyRepositoryMock.Setup(repo => repo.FetchTopStoryIdsAsync())
                .ReturnsAsync(new[] { 1, 2, 3 });

            // Act
            var result = await _service.GetTopStoryIdsAsync();

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, result);
        }

        [Fact]
        public async Task GetStoriesAsync_FetchesAndCachesValidStories()
        {
            // Arrange
            var storyIds = new[] { 1, 2, 3 };
            var validStories = new List<StoryModel>
            {
                new StoryModel { Id = 1, Title = "Story 1", Url = "http://story1.com" },
                new StoryModel { Id = 2, Title = "Story 2", Url = "http://story2.com" }
            };

            // Mock repository behavior
            _storyRepositoryMock.Setup(repo => repo.FetchTopStoryIdsAsync())
                .ReturnsAsync(storyIds);

            _storyRepositoryMock.Setup(repo => repo.FetchStoryDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => validStories.FirstOrDefault(s => s.Id == id) ?? new StoryModel
                {
                    Id = id,
                    Title = "Unknown",
                    Url = string.Empty
                });

            // Mock cache behavior
            object? cacheValue = null;
            _memoryCacheMock.Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _memoryCacheMock.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetTopStoriesAsync(1, 10, "");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _storyRepositoryMock.Verify(repo => repo.FetchTopStoryIdsAsync(), Times.Once);
            _storyRepositoryMock.Verify(repo => repo.FetchStoryDetailsAsync(It.IsAny<int>()), Times.Exactly(3));
        }
    }
}
