using Route = TestApp.Models.Route;

namespace TestApp.Caching
{
    // This interface defines the contract for a cache of routes.
    public interface IRouteCache
    {
        // Gets all routes stored in the cache.
        List<Route> GetRoutes();

        // Adds a list of routes to the cache.
        void AddRoutes(List<Route>? routes);
    }
}
