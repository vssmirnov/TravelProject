namespace TestApp.Models
{
    public class ProviderOneRoute
    {
        /// <summary>
        /// Mandatory
        /// Start point of route
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Mandatory
        /// Start date of route
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Mandatory
        /// End date of route
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Mandatory
        /// Price of route
        // /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Mandatory
        /// Timelimit. After it expires, route became not actual
        /// </summary>
        public DateTime TimeLimit { get; set; }
    }
}
