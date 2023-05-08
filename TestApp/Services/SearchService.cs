using TestApp.Caching;
using TestApp.CustomException;
using TestApp.Models;
using Route = TestApp.Models.Route;

namespace TestApp.Services
{
    public class SearchService : ISearchService
    {
        private readonly IProviderOneSearchService _providerOneSearchService;
        private readonly IProviderTwoSearchService _providerTwoSearchService;
        private readonly IRouteCache _routeCache;

        public SearchService(IProviderOneSearchService providerOneSearchService,
            IProviderTwoSearchService providerTwoSearchService,
            IRouteCache routeCache)
        {
            _providerOneSearchService = providerOneSearchService;
            _providerTwoSearchService = providerTwoSearchService;
            _routeCache = routeCache;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            // Check if both providers are available
            var providerOneIsAvailable = await _providerOneSearchService.IsAvailableAsync(cancellationToken);
            var providerTwoIsAvailable = await _providerTwoSearchService.IsAvailableAsync(cancellationToken);

            if (!providerOneIsAvailable && !providerTwoIsAvailable && request.Filters?.OnlyCached != true)
                throw new ServiceUnavailableException("Both providers are unavailable");

            // Search routes from provider one
            ProviderOneSearchResponse providerOneResponse = null!;
            if (providerOneIsAvailable)
            {
                var providerOneRequest = new ProviderOneSearchRequest
                {
                    From = request.Origin,
                    To = request.Destination,
                    DateFrom = request.OriginDateTime,
                    DateTo = request.Filters?.DestinationDateTime,
                    MaxPrice = request.Filters?.MaxPrice
                };

                providerOneResponse =
                    await _providerOneSearchService.SearchAsync(providerOneRequest, cancellationToken);
            }

            // Search routes from provider two
            ProviderTwoSearchResponse providerTwoResponse = null!;
            if (providerTwoIsAvailable)
            {
                var providerTwoRequest = new ProviderTwoSearchRequest
                {
                    Departure = request.Origin,
                    Arrival = request.Destination,
                    DepartureDate = request.OriginDateTime,
                    MinTimeLimit = request.Filters?.MinTimeLimit
                };

                providerTwoResponse =
                    await _providerTwoSearchService.SearchAsync(providerTwoRequest, cancellationToken);
            }

            // Merge and filter routes from both providers
            var mergedRoutes = MergeRoutes(providerOneResponse?.Routes ?? new ProviderOneRoute[]{},
                providerTwoResponse?.Routes ?? new ProviderTwoRoute[]{}, request.Filters?.OnlyCached ?? false);
            var filteredRoutes = FilterRoutes(mergedRoutes, request.Filters);

            // Add routes to cache
            _routeCache.AddRoutes(filteredRoutes);

            // Get cheapest and fastest routes
            var cheapestRoute = filteredRoutes.MinBy(r => r.Price);
            var fastestRoute = filteredRoutes.MinBy(r => (r.DestinationDateTime - r.OriginDateTime).TotalMinutes);

            return new SearchResponse
            {
                Routes = filteredRoutes.ToArray(),
                MinPrice = cheapestRoute?.Price ?? 0,
                MaxPrice = fastestRoute?.Price ?? 0,
                MinTravelTime = filteredRoutes.Any() ? (int)filteredRoutes.Min(r => (r.DestinationDateTime - r.OriginDateTime).TotalMinutes) : Int32.MaxValue,
                MaxTravelTime = filteredRoutes.Any() ? (int)filteredRoutes.Max(r => (r.DestinationDateTime - r.OriginDateTime).TotalMinutes) : Int32.MinValue
            };
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true); // Always return true, because the service is always available
        }

        private List<Route> MergeRoutes(ProviderOneRoute[] providerOneRoutes,
            ProviderTwoRoute[] providerTwoRoutes, bool filtersOnlyCached)
        {
            var mergedRoutes = new List<Route>();

            if (filtersOnlyCached)
                return _routeCache.GetRoutes();

            // Add routes from provider one
            if (providerOneRoutes != null)
                foreach (var providerOneRoute in providerOneRoutes)
                {
                    mergedRoutes.Add(new Route
                    {
                        Id = Guid.NewGuid(),
                        Origin = providerOneRoute.From,
                        Destination = providerOneRoute.To,
                        OriginDateTime = providerOneRoute.DateFrom,
                        DestinationDateTime = providerOneRoute.DateTo,
                        Price = providerOneRoute.Price,
                        TimeLimit = providerOneRoute.TimeLimit
                    });
                }

            // Add routes from provider two
            if (providerTwoRoutes != null)
                foreach (var providerTwoRoute in providerTwoRoutes)
                {
                    mergedRoutes.Add(new Route
                    {
                        Id = Guid.NewGuid(),
                        Origin = providerTwoRoute.Departure.Point,
                        Destination = providerTwoRoute.Arrival.Point,
                        OriginDateTime = providerTwoRoute.Departure.Date,
                        DestinationDateTime = providerTwoRoute.Arrival.Date,
                        Price = providerTwoRoute.Price,
                        TimeLimit = providerTwoRoute.TimeLimit
                    });
                }

            // Sort routes by price
            mergedRoutes.Sort((a, b) => a.Price.CompareTo(b.Price));

            return mergedRoutes;
        }

        private List<Route> FilterRoutes(List<Route> routes, SearchFilters? filters)
        {
            if (filters == null)
            {
                return routes;
            }

            var filteredRoutes = routes.Where(route =>
                (filters.DestinationDateTime == null || route.DestinationDateTime <= filters.DestinationDateTime) &&
                (filters.MaxPrice == null || route.Price <= filters.MaxPrice) &&
                (filters.MinTimeLimit == null || route.TimeLimit >= filters.MinTimeLimit));

            return filteredRoutes.ToList();
        }

    }
}