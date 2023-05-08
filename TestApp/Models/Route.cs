namespace TestApp.Models
{
    public class Route
    {
        /// <summary>
        /// Mandatory
        /// Identifier of the whole route
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mandatory
        /// Start point of route
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Mandatory
        /// End point of route
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Mandatory
        /// Start date of route
        /// </summary>
        public DateTime OriginDateTime { get; set; }

        /// <summary>
        /// Mandatory
        /// End date of route
        /// </summary>
        public DateTime DestinationDateTime { get; set; }

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
