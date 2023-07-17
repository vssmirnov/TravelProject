using TestApp.Models;

namespace TestApp.Caching
{
    // This interface defines the contract for a cache of routes.
    public interface IRouteCache
    {
        // Gets routes from the cache based on a search request.
        List<Models.Route> GetRoutes(SearchRequest request);

        // Adds a list of routes to the cache.
        void AddRoutes(SearchRequest request, List<Models.Route> filteredRoutes);
    }
}