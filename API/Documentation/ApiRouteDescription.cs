namespace HarvestChoiceApi.Documentation.Models
{
    using System.Web.Http.Description;

    /// <summary>
    /// Documentation for an API route
    /// </summary>
    public class ApiRouteDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRouteDescription" /> class.
        /// </summary>
        public ApiRouteDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRouteDescription" /> class.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        public ApiRouteDescription(ApiDescription apiDescription)
        {
            this.Method = apiDescription.HttpMethod.Method;
            this.Path = apiDescription.RelativePath;
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }
    }
}