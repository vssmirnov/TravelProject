using System.Net;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TestApp.Models;
using TestApp.Services;

namespace TestApp.Tests.Services
{
    public class ProviderTwoSearchServiceTests
    {
        [Fact]
        public async Task SearchAsync_Returns_Correct_Response()
        {
            // Arrange
            var expectedResponse = new ProviderTwoSearchResponse
            {
                Routes = new ProviderTwoRoute[]
                {
                    new()
                    {
                        Arrival = new ProviderTwoPoint(){Point = "City A", Date = DateTime.UtcNow} ,
                        Departure = new ProviderTwoPoint(){Point = "City B", Date = DateTime.UtcNow},
                        Price = 150,
                        TimeLimit = DateTime.UtcNow.AddDays(7)
                    }
                }
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
                });

            var httpClient = new HttpClient(httpMessageHandler.Object);

            var providerOneSearchService = new ProviderTwoSearchService(httpClient);

            // Act
            var actualResponse = await providerOneSearchService.SearchAsync(new ProviderTwoSearchRequest(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse.Routes.Length, actualResponse.Routes.Length);
            Assert.Equal(expectedResponse.Routes[0].Arrival.Point, actualResponse.Routes[0].Arrival.Point);
            Assert.Equal(expectedResponse.Routes[0].Departure.Point, actualResponse.Routes[0].Departure.Point);
            Assert.Equal(expectedResponse.Routes[0].Arrival.Date, actualResponse.Routes[0].Arrival.Date);
            Assert.Equal(expectedResponse.Routes[0].Departure.Date, actualResponse.Routes[0].Departure.Date);
            Assert.Equal(expectedResponse.Routes[0].Price, actualResponse.Routes[0].Price);
            Assert.Equal(expectedResponse.Routes[0].TimeLimit, actualResponse.Routes[0].TimeLimit);
        }

        [Fact]
        public async Task IsAvailableAsync_Returns_True_When_Api_Client_Is_Available()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);
            var searchService = new ProviderTwoSearchService(httpClientMock.Object);

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Setup(_ => _.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(expectedResponse);

            // Act
            var isAvailable = await searchService.IsAvailableAsync(default);

            // Assert
            Assert.True(isAvailable);
        }

        [Fact]
        public async Task IsAvailableAsync_Returns_False_When_Api_Client_Is_Not_Available()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);
            var searchService = new ProviderTwoSearchService(httpClientMock.Object);

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);

            httpClientMock
                .Setup(_ => _.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(expectedResponse);

            // Act
            var isAvailable = await searchService.IsAvailableAsync(default);

            // Assert
            Assert.False(isAvailable);
        }
    }
}
