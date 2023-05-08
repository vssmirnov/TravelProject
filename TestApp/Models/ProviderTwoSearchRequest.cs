namespace TestApp.Models
{
    public class ProviderTwoSearchRequest
    {
        /// <summary>
        /// Mandatory
        /// Start point of route, e.g. Moscow 
        /// </summary>
        public string Departure { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route, e.g. Sochi
        /// </summary>
        public string Arrival { get; set; }

        /// <summary>
        /// Mandatory
        /// Start date of route
        /// </summary>
        public DateTime DepartureDate { get; set; }

        /// <summary>
        /// Optional
        /// Minimum value of timelimit for route
        /// </summary>
        public DateTime? MinTimeLimit { get; set; }
    }
}
