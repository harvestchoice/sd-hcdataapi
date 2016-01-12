namespace HarvestChoiceApi.Documentation.Models
{
    using System.Collections.Generic;
    using System.Web.Http;

    /// <summary>
    /// Documentation for an <see cref="ApiController"/>
    /// </summary>
    public class ApiControllerDescription
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public IList<ApiActionDescription> Actions { get; set; }
    }
}