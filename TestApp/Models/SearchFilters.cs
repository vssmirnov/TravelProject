namespace TestApp.Models
{
    public class SearchFilters
    {
        /// <summary>
        /// Optional
        /// End date of route
        /// </summary>
        public DateTime? DestinationDateTime { get; set; }

        /// <summary>
        /// Optional
        /// Maximum price of route
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Optional
        /// Minimum value of timelimit for route
        /// </summary>
        public DateTime? MinTimeLimit { get; set; }

        /// <summary>
        /// Optional
        /// Forcibly search in cached data
        /// </summary>
        public bool? OnlyCached { get; set; }
    }
}
