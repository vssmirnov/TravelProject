using Moq;
using TestApp.Caching;
using TestApp.CustomException;
using TestApp.Models;
using TestApp.Services;

namespace TestApp.Tests.Services
{
    public class SearchServiceTests
    {
        private readonly SearchService _searchService;
        private readonly Mock<IRouteCache> _cacheMock;
        private readonly Mock<IProviderOneSearchService> _providerOneMock;
        private readonly Mock<IProviderTwoSearchService> _providerTwoMock;
        private readonly string _cityA = "City A";
        private readonly string _cityB = "City B";
        private readonly string _cityC = "City C";

        public SearchServiceTests()
        {
            _cacheMock = new Mock<IRouteCache>();
            _providerOneMock = new Mock<IProviderOneSearchService>();
            _providerTwoMock = new Mock<IProviderTwoSearchService>();
            _searchService = new SearchService(_providerOneMock.Object, _providerTwoMock.Object, _cacheMock.Object);
        }
        
        [Fact]
        public async Task SearchRoutes_Returns_Routes_From_Both_Providers()
        {
            // Arrange
            var providerOneResponse = new ProviderOneSearchResponse
            {
                Routes = new ProviderOneRoute[]
                {
                    new()
                    {
                        From = _cityA,
                        To = _cityB,
                        DateFrom = DateTime.Today.AddDays(1),
                        DateTo = DateTime.Today.AddDays(2),
                        Price = 100,
                        TimeLimit = DateTime.Today.AddDays(3)
                    }
                }
            };

            var providerTwoResponse = new ProviderTwoSearchResponse
            {
                Routes = new ProviderTwoRoute[]
                {
                    new()
                    {
                        Departure = new ProviderTwoPoint
                        {
                            Point = _cityA,
                            Date = DateTime.Today.AddDays(1)
                        },
                        Arrival = new ProviderTwoPoint
                        {
                            Point = _cityB,
                            Date = DateTime.Today.AddDays(2)
                        },
                        Price = 200,
                        TimeLimit = DateTime.Today.AddDays(3)
                    }
                }
            };

            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            var filters = new SearchFilters
            {
                DestinationDateTime = DateTime.Today.AddDays(2),
                MaxPrice = 250,
                MinTimeLimit = DateTime.Today.AddDays(1),
                OnlyCached = false
            };

            var searchRequest = new SearchRequest()
            {
                Destination = _cityB,
                Origin = _cityA,
                Filters = filters,
                OriginDateTime = DateTime.Today.AddDays(1)
            };

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Routes);

            var expectedRoutes = new List<Route>
            {
                new()
                {
                    Origin = _cityA,
                    Destination = _cityB,
                    OriginDateTime = DateTime.Today.AddDays(1),
                    DestinationDateTime = DateTime.Today.AddDays(2),
                    Price = 100
                },
                new()
                {
                    Origin = _cityA,
                    Destination = _cityB,
                    OriginDateTime = DateTime.Today.AddDays(1),
                    DestinationDateTime = DateTime.Today.AddDays(2),
                    Price = 200
                }
            };

            Assert.Equal(expectedRoutes.Count, result.Routes.Length);
            for (var i = 0; i < result.Routes.Length; i++)
            {
                Assert.Equal(expectedRoutes[i].Origin, result.Routes[i].Origin);
                Assert.Equal(expectedRoutes[i].Destination, result.Routes[i].Destination);
                Assert.Equal(expectedRoutes[i].OriginDateTime, result.Routes[i].OriginDateTime);
                Assert.Equal(expectedRoutes[i].DestinationDateTime, result.Routes[i].DestinationDateTime);
                Assert.Equal(expectedRoutes[i].Price, result.Routes[i].Price);
            }

            _providerOneMock.Verify(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None),
                Times.Once);
            _providerTwoMock.Verify(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task SearchRoutes_Returns_Routes_From_Provider_One()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = DateTime.Today.AddDays(7)
            };
            var providerOneResponse = new ProviderOneSearchResponse
            {
                Routes = new ProviderOneRoute[]
                {
                    new()
                    {
                        From = _cityA,
                        To = _cityB,
                        DateFrom = DateTime.Today,
                        DateTo = DateTime.Today.AddDays(7),
                        Price = 100,
                        TimeLimit = DateTime.Today.AddDays(1)
                    },
                    new()
                    {
                        From = _cityA,
                        To = _cityB,
                        DateFrom = DateTime.Today.AddDays(1),
                        DateTo = DateTime.Today.AddDays(8),
                        Price = 150,
                        TimeLimit = DateTime.Today.AddDays(2)
                    }
                }
            };
            var providerTwoResponse = new ProviderTwoSearchResponse
            {
                Routes = new ProviderTwoRoute[]{}
            };
            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(false);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Routes.Length);
            Assert.Equal(_cityA, result.Routes[0].Origin);
            Assert.Equal(_cityB, result.Routes[0].Destination);
            Assert.Equal(DateTime.Today, result.Routes[0].OriginDateTime);
            Assert.Equal(DateTime.Today.AddDays(7), result.Routes[0].DestinationDateTime);
            Assert.Equal(100, result.Routes[0].Price);
            Assert.Equal(DateTime.Today.AddDays(1), result.Routes[0].TimeLimit);
            Assert.Equal(_cityA, result.Routes[1].Origin);
            Assert.Equal(_cityB, result.Routes[1].Destination);
            Assert.Equal(DateTime.Today.AddDays(1), result.Routes[1].OriginDateTime);
            Assert.Equal(DateTime.Today.AddDays(8), result.Routes[1].DestinationDateTime);
            Assert.Equal(150, result.Routes[1].Price);
            Assert.Equal(DateTime.Today.AddDays(2), result.Routes[1].TimeLimit);
        }

        [Fact]
        public async Task SearchRoutes_Returns_Routes_From_Provider_Two()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = DateTime.Today.AddDays(1)
            };

            var providerTwoResponse = new ProviderTwoSearchResponse
            {
                Routes = new ProviderTwoRoute[]
                {
                    new()
                    {
                        Departure = new ProviderTwoPoint { Point = _cityA, Date = DateTime.UtcNow },
                        Arrival = new ProviderTwoPoint { Point = _cityB, Date = DateTime.UtcNow.AddDays(1) },
                        Price = 100.0M,
                        TimeLimit = DateTime.UtcNow.AddDays(3)
                    }
                }
            };

            var providerOneResponse = new ProviderOneSearchResponse
            {
                Routes = new ProviderOneRoute[]{}
            };

            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).Returns(Task.FromResult(providerOneResponse));
            _providerOneMock.Setup(t => t.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(false);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).Returns(Task.FromResult(providerTwoResponse));
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Routes);
            Assert.Single(result.Routes);
            Assert.Equal(_cityA, result.Routes[0].Origin);
            Assert.Equal(_cityB, result.Routes[0].Destination);
        }

        [Fact]
        public async Task SearchRoutes_Returns_No_Routes_When_Both_Providers_Fail()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = DateTime.Now.AddDays(3),
                Filters = new SearchFilters
                {
                    MaxPrice = 1000
                }
            };

            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(false);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(false);

            // Act
            try
            {
                // Act
                var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

                // Assert
                Assert.Empty(result.Routes);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<ServiceUnavailableException>(ex);
            }
        }

        [Fact]
        public async Task SearchRoutes_Returns_No_Routes_When_All_Providers_Return_No_Routes()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = DateTime.Now.AddDays(1),
                Filters = new SearchFilters()
            };

            var providerOneResponse = new ProviderOneSearchResponse { Routes = new ProviderOneRoute[]{} };
            var providerTwoResponse = new ProviderTwoSearchResponse { Routes = new ProviderTwoRoute[]{} };

            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.Empty(result.Routes);
        }

        [Fact]
        public async Task Test_FilterRoutes_Returns_Expected_Number_Of_Routes()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = new DateTime(2022, 01, 02),
                Filters = new SearchFilters() { MaxPrice = 50 }
            };

            var providerOneRoute = new ProviderOneRoute
            {
                From = _cityA,
                To = _cityB,
                DateFrom = new DateTime(2022, 01, 01),
                DateTo = new DateTime(2022, 01, 02),
                Price = 50,
                TimeLimit = new DateTime(2021, 12, 31)
            };

            var providerTwoRoute = new ProviderTwoRoute
            {
                Departure = new ProviderTwoPoint { Point = _cityA, Date = new DateTime(2022, 01, 01) },
                Arrival = new ProviderTwoPoint { Point = _cityB, Date = new DateTime(2022, 01, 02) },
                Price = 75,
                TimeLimit = new DateTime(2021, 12, 31)
            };

            var providerOneResponse = new ProviderOneSearchResponse { Routes = new[] { providerOneRoute } };
            var providerTwoResponse = new ProviderTwoSearchResponse { Routes = new[] { providerTwoRoute } };
            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            // Act
            var searchResult = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.Single(searchResult.Routes);
        }

        [Fact]
        public async Task Test_FilterRoutes_Returns_Expected_Routes_When_Filtering_By_DestinationDateTime()
        {
            // Arrange
            var providerOneRoutes = new ProviderOneRoute[]
            {
                new()
                {
                    From = _cityA, To = _cityB, DateFrom = DateTime.Now, DateTo = DateTime.Now.AddDays(6),
                    Price = 500, TimeLimit = DateTime.Now.AddDays(6)
                },
                new()
                {
                    From = _cityB, To = _cityA, DateFrom = DateTime.Now, DateTo = DateTime.Now.AddDays(5),
                    Price = 400, TimeLimit = DateTime.Now.AddDays(5)
                },
                new()
                {
                    From = _cityA, To = _cityC, DateFrom = DateTime.Now, DateTo = DateTime.Now.AddDays(6),
                    Price = 300, TimeLimit = DateTime.Now.AddDays(6)
                }
            };
            var providerOneResponse = new ProviderOneSearchResponse { Routes = providerOneRoutes };

            var providerTwoRoutes = new ProviderTwoRoute[]
            {
                new()
                {
                    Departure = new ProviderTwoPoint { Point = _cityA, Date = DateTime.Now },
                    Arrival = new ProviderTwoPoint { Point = _cityB, Date = DateTime.Now.AddDays(6) }, Price = 700,
                    TimeLimit = DateTime.Now.AddDays(6)
                },
                new()
                {
                    Departure = new ProviderTwoPoint { Point = _cityB, Date = DateTime.Now },
                    Arrival = new ProviderTwoPoint { Point = _cityA, Date = DateTime.Now.AddDays(5) }, Price = 800,
                    TimeLimit = DateTime.Now.AddDays(5)
                },
                new()
                {
                    Departure = new ProviderTwoPoint { Point = _cityA, Date = DateTime.Now },
                    Arrival = new ProviderTwoPoint { Point = _cityC, Date = DateTime.Now.AddDays(6) }, Price = 900,
                    TimeLimit = DateTime.Now.AddDays(6)
                }
            };
            var providerTwoResponse = new ProviderTwoSearchResponse { Routes = providerTwoRoutes };

            var searchFilters = new SearchFilters { DestinationDateTime = DateTime.Now.AddDays(5) };
            var searchRequest = new SearchRequest { Filters = searchFilters };

            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Routes.Length);
            Assert.Equal(_cityA, result.Routes[0].Destination);
            Assert.Equal(_cityB, result.Routes[1].Origin);
        }

        [Fact]
        public async Task Test_FilterRoutes_Returns_Expected_Routes_When_Filtering_By_MinTimeLimit()
        {
            // Arrange
            var searchRequest = new SearchRequest
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = DateTime.UtcNow,
                Filters = new SearchFilters() { MinTimeLimit = DateTime.UtcNow.AddHours(1) }
            };

            var providerOneRoute = new ProviderOneRoute
            {
                From = _cityA,
                To = _cityB,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(1),
                Price = 100,
                TimeLimit = DateTime.UtcNow
            };

            var providerTwoRoute = new ProviderTwoRoute
            {
                Departure = new ProviderTwoPoint { Point = _cityA, Date = DateTime.UtcNow },
                Arrival = new ProviderTwoPoint { Point = _cityB, Date = DateTime.UtcNow.AddDays(1) },
                Price = 200,
                TimeLimit = DateTime.UtcNow.AddHours(2)
            };

            var providerOneResponse = new ProviderOneSearchResponse
            {
                Routes = new[] { providerOneRoute }
            };

            var providerTwoResponse = new ProviderTwoSearchResponse
            {
                Routes = new[] { providerTwoRoute }
            };

            _providerOneMock.Setup(p => p.SearchAsync(It.IsAny<ProviderOneSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerOneResponse);
            _providerOneMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);
            _providerTwoMock.Setup(p => p.SearchAsync(It.IsAny<ProviderTwoSearchRequest>(), CancellationToken.None)).ReturnsAsync(providerTwoResponse);
            _providerTwoMock.Setup(p => p.IsAvailableAsync(CancellationToken.None)).ReturnsAsync(true);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.Single(result.Routes);
            Assert.Equal(providerTwoRoute.Price, result.Routes.First().Price);
        }

        [Fact]
        public async Task Test_FilterRoutes_Returns_Expected_Routes_When_Filtering_By_OnlyCached()
        {
            // Arrange
            var routes = new List<Route>()
            {
                new()
                {
                    Origin = _cityA,
                    Destination = _cityB,
                    OriginDateTime = new DateTime(2022, 6, 1, 8, 0, 0),
                    DestinationDateTime = new DateTime(2022, 6, 1, 10, 0, 0),
                    Price = 100,
                    TimeLimit = new DateTime(2022, 5, 30)
                },
                new()
                {
                    Origin = _cityB,
                    Destination = _cityA,
                    OriginDateTime = new DateTime(2022, 6, 1, 8, 0, 0),
                    DestinationDateTime = new DateTime(2022, 6, 1, 10, 0, 0),
                    Price = 200,
                    TimeLimit = new DateTime(2022, 5, 30)
                }
            };

            var searchRequest = new SearchRequest()
            {
                Origin = _cityA,
                Destination = _cityB,
                OriginDateTime = new DateTime(2022, 6, 1, 8, 0, 0),
                Filters = new SearchFilters() { OnlyCached = true }
            };

            _cacheMock.Setup(c => c.GetRoutes(It.Is<SearchRequest>(r => r.Origin == _cityA && r.Destination == _cityB && r.OriginDateTime == new DateTime(2022, 6, 1, 8, 0, 0)))).Returns(routes);

            // Act
            var result = await _searchService.SearchAsync(searchRequest, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Routes.Length);
            Assert.Equal(_cityA, result.Routes[0].Origin);
            Assert.Equal(_cityB, result.Routes[0].Destination);
            Assert.Equal(_cityB, result.Routes[1].Origin);
            Assert.Equal(_cityA, result.Routes[1].Destination);
        }

    }
}
