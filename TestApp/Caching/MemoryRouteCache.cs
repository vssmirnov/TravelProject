using System.Runtime.Caching;
using TestApp.Models;
using Route = TestApp.Models.Route;
using MemoryCache = System.Runtime.Caching.MemoryCache;

namespace TestApp.Caching
{
    public class MemoryRouteCache : IRouteCache
    {
        private readonly ObjectCache _cache;
        private readonly CacheItemPolicy _policy;

        public MemoryRouteCache(TimeSpan ttl)
        {
            _cache = MemoryCache.Default;

            _policy = new CacheItemPolicy
            {
                SlidingExpiration = ttl
            };
        }

        public List<Route> GetRoutes(SearchRequest request)
        {
            var cacheKey = GetCacheKey(request);
            return _cache.Get(cacheKey) as List<Route>;
        }

        public void AddRoutes(SearchRequest request, List<Route> routes)
        {
            var cacheKey = GetCacheKey(request);
            _cache.Add(cacheKey, routes, _policy);
        }

        private string GetCacheKey(SearchRequest request)
        {
            return $"{request.Origin}-{request.Destination}-{request.OriginDateTime:yyyy-MM-dd}";
        }
    }
}