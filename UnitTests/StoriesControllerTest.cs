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
    }
}