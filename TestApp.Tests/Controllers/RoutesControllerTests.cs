using Microsoft.AspNetCore.Mvc;
using Moq;
using TestApp.Controllers;
using TestApp.CustomException;
using TestApp.Models;
using TestApp.Services;

namespace TestApp.Tests.Controllers
{
    public class RoutesControllerTests
    {
        private readonly Mock<ISearchService> _searchServiceMock;
        private readonly RoutesController _controller;

        public RoutesControllerTests()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _controller = new RoutesController(_searchServiceMock.Object);
        }

        [Fact]
        public async Task SearchAsync_Returns_OkObjectResult_With_SearchResponse()
        {
            // Arrange
            var searchServiceMock = new Mock<ISearchService>();
            var controller = new RoutesController(searchServiceMock.Object);

            var request = new SearchRequest();

            var searchResponse = new SearchResponse
            {
                Routes = new Route[] { },
                MinPrice = 0,
                MaxPrice = 0,
                MinTravelTime = 0,
                MaxTravelTime = 0
            };

            searchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResponse);

            // Act
            var response = await controller.SearchAsync(request, CancellationToken.None);

            // Assert
            Assert.IsType<ActionResult<SearchResponse>>(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response.Result);
            var result = Assert.IsType<SearchResponse>(okObjectResult.Value);
            Assert.Equal(searchResponse, result);
        }

        [Fact]
        public async Task SearchAsync_Returns_Status500_When_ServiceUnavailableException_Is_Thrown()
        {
            // Arrange
            var searchRequest = new SearchRequest();
            _searchServiceMock.Setup(x => x.SearchAsync(searchRequest, default))
                .ThrowsAsync(new ServiceUnavailableException(""));

            // Act
            var response = await _controller.SearchAsync(searchRequest, default);

            // Assert
            Assert.IsType<ActionResult<SearchResponse>>(response);
            var objectResult = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Both providers are unavailable", objectResult.Value);
        }
    }
}
