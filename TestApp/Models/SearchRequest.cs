namespace TestApp.Models
{
    public class SearchRequest
    {
        /// <summary>
        /// Mandatory
        /// Start point of route, e.g. Moscow 
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route, e.g. Sochi
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Mandatory
        /// Start date of route
        /// </summary>
        public DateTime OriginDateTime { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public SearchFilters? Filters { get; set; }
    }
}
