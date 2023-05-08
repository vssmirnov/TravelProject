namespace TestApp.Models
{
    public class ProviderTwoRoute
    {
        /// <summary>
        /// Mandatory
        /// Start point of route
        /// </summary>
        public ProviderTwoPoint Departure { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route
        /// </summary>
        public ProviderTwoPoint Arrival { get; set; }

        /// <summary>
        /// Mandatory
        /// Price of route
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Mandatory
        /// Timelimit. After it expires, route became not actual
        /// </summary>
        public DateTime TimeLimit { get; set; }
    }
}
