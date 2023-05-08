namespace TestApp.Models
{
    public class ProviderOneSearchRequest
    {
        /// <summary>
        /// Mandatory
        /// Start point of route, e.g. Moscow 
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route, e.g. Sochi
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Mandatory
        /// Start date of route
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Optional
        /// End date of route
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Optional
        /// Maximum price of route
        /// </summary>
        public decimal? MaxPrice { get; set; }
    }
}
