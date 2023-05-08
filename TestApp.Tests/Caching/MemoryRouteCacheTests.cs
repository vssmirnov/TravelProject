using TestApp.Models;
using TestApp.Caching;

namespace TestApp.Tests.Caching
{
    public class MemoryRouteCacheTests
    {
        [Fact]
        public void AddRoutes_Adds_Routes_To_Cache()
        {
            // Arrange
            var cache = new MemoryRouteCache();
            var routes = new List<Route>
            {
                new() { Id = Guid.NewGuid(), Origin = "A", Destination = "B", OriginDateTime = DateTime.Now, DestinationDateTime = DateTime.Now.AddHours(1), Price = 100, TimeLimit = DateTime.Now.AddHours(2) },
                new() { Id = Guid.NewGuid(), Origin = "B", Destination = "C", OriginDateTime = DateTime.Now, DestinationDateTime = DateTime.Now.AddHours(2), Price = 200, TimeLimit = DateTime.Now.AddHours(3) }
            };

            // Act
            cache.AddRoutes(routes);

            // Assert
            Assert.Equal(routes.Count, cache.GetRoutes().Count);
            Assert.Equal(routes[0], cache.GetRoutes().First(t => t.Id == routes[0].Id));
            Assert.Equal(routes[1], cache.GetRoutes().First(t => t.Id == routes[1].Id));
        }

        [Fact]
        public void TryGet_Returns_True_If_Route_Is_In_Cache()
        {
            // Arrange
            var cache = new MemoryRouteCache();
            var route = new Route { Id = Guid.NewGuid(), Origin = "A", Destination = "B", OriginDateTime = DateTime.Now, DestinationDateTime = DateTime.Now.AddHours(1), Price = 100, TimeLimit = DateTime.Now.AddHours(2) };
            cache.AddRoutes(new List<Route> { route });

            // Act
            var result = cache.GetRoutes();

            // Assert
            Assert.True(result.Any());
            Assert.Equal(route.Destination, result[0].Destination);
            Assert.Equal(route.Origin, result[0].Origin);
            Assert.Equal(route.DestinationDateTime, result[0].DestinationDateTime);
            Assert.Equal(route.OriginDateTime, result[0].OriginDateTime);
            Assert.Equal(route.Price, result[0].Price);
            Assert.Equal(route.TimeLimit, result[0].TimeLimit);
            Assert.Equal(route.Id, result[0].Id);
        }

        [Fact]
        public void TryGet_Returns_False_If_Route_Is_Not_In_Cache()
        {
            // Arrange
            var cache = new MemoryRouteCache();

            // Act
            var result = cache.GetRoutes();

            // Assert
            Assert.False(result.Any());
            Assert.NotNull(result);
        }
    }
}
