using System.Collections.Concurrent;
using Route = TestApp.Models.Route;

namespace TestApp.Caching
{
    public class MemoryRouteCache : IRouteCache
    {
        private readonly ConcurrentDictionary<Guid, Route> _routes;

        public MemoryRouteCache()
        {
            _routes = new ConcurrentDictionary<Guid, Route>();
        }

        public List<Route> GetRoutes()
        {
            return _routes.Values.ToList();
        }

        public void AddRoutes(List<Route>? routes)
        {
            if (routes == null) return;
            foreach (var route in routes)
            {
                _routes.AddOrUpdate(route.Id, route, (_, __) => route);
            }
        }
    }
}
