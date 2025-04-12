using DataRepoLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NewsApi.Controllers;
using ServiceLayer;
using ViewModel;

namespace UnitTests
{
    public class StoriesControllerTest
    {
        [Fact]
        public async Task GetTopStoriesAsync_ReturnsOkResultWithStories()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoriesController>>();
            var mockStoriesService = new Mock<IStoriesService>();
            var sampleStories = new List<StoryModel>
            {
                new StoryModel { Id = 1, Title = "Story1", Url = "http://example.com/story1" },
                new StoryModel { Id = 2, Title = "Story2", Url = "http://example.com/story2" },
                new StoryModel { Id = 3, Title = "Story3", Url = "http://example.com/story3" }
            };

            mockStoriesService
                .Setup(s => s.GetTopStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(sampleStories);

            var controller = new StoriesController(mockLogger.Object, mockStoriesService.Object);

            // Act
            var result = await controller.GetTopStoriesAsync(1, 10, "sample");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStories = Assert.IsType<List<StoryModel>>(okResult.Value);
            Assert.Equal(sampleStories, returnedStories);
        }

        [Fact]
        public async Task GetTopStoriesAsync_ReturnsBadRequest_WhenParametersAreInvalid()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoriesController>>();
            var mockStoriesService = new Mock<IStoriesService>();
            var controller = new StoriesController(mockLogger.Object, mockStoriesService.Object);

            // Act
            var result = await controller.GetTopStoriesAsync(0, 0, "");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTopStoryIdsAsync_CacheReturnsData_ShouldReturnCachedData()
        {
            // Arrange
            var mockMemoryCache = new Mock<IMemoryCache>();
            var mockStoryRepository = new Mock<IStoryRepository>();
            var mockCacheEntry = Mock.Of<ICacheEntry>();
            var cachedStoryIds = new int[] { 1, 2, 3, 4, 5 }; // Mocked cached data

            mockStoryRepository
            .Setup(repo => repo.FetchTopStoryIdsAsync())
            .ReturnsAsync(cachedStoryIds);

            // Mock GetOrCreateAsync functionality
            mockMemoryCache
                .Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry);

            mockStoryRepository
                .Setup(repo => repo.FetchTopStoryIdsAsync())
                .ReturnsAsync(cachedStoryIds);

            var apiSettings = new ApiSettings { MaxStories = 100 };
            var cacheSettings = new CacheSettings
            {
                AbsoluteExpirationMinutes = 10,
                SlidingExpirationMinutes = 5
            };

            var service = new StoriesService(
                mockStoryRepository.Object,
                mockMemoryCache.Object,
                Options.Create(cacheSettings),
                Options.Create(apiSettings)
            );

            // Act
            var result = await service.GetTopStoryIdsAsync();

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Equal(cachedStoryIds.Length, result.Length); // Verify length matches
            Assert.Equal(cachedStoryIds, result); // Verify the data matches cached data
        }

        [Fact]
        public async Task GetTopStoryIdsAsync_CacheMiss_ShouldFetchFromRepository()
        {
            // Arrange
            var mockMemoryCache = new Mock<IMemoryCache>();
            var mockStoryRepository = new Mock<IStoryRepository>();
            var apiSettings = new ApiSettings { MaxStories = 100 };
            var cacheSettings = new CacheSettings();

            var repositoryData = new int[] { 6, 7, 8, 9, 10 }; // Simulated data from repository

            // Simulate cache behavior
            object? cacheValue = null;
            mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out cacheValue))
                           .Returns(false);


            mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                           .Returns(() =>
                           {
                               var cacheEntry = new Mock<ICacheEntry>();
                               return cacheEntry.Object;
                           });

            mockStoryRepository.Setup(repo => repo.FetchTopStoryIdsAsync())
                               .ReturnsAsync(repositoryData);

            var service = new StoriesService(
                mockStoryRepository.Object,
                mockMemoryCache.Object,
                Options.Create(cacheSettings),
                Options.Create(apiSettings)
            );

            // Act
            var result = await service.GetTopStoryIdsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(repositoryData.Length, result.Length);
            Assert.Equal(repositoryData, result);
        }
    }
}