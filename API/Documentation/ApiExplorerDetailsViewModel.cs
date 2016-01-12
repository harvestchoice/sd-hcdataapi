namespace HarvestChoiceApi.Documentation.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Web.Http.Description;

    using HarvestChoiceApi.Documentation.Models;

    /// <summary>
    /// Viewmodel for the detailed API view.
    /// </summary>
    public class ApiExplorerDetailsViewModel : ApiExplorerViewModel
    {
        /// <summary>
        /// Gets or sets the API controller description.
        /// </summary>
        /// <value>
        /// The API controller description.
        /// </value>
        public ApiControllerDescription ApiControllerDescription { get; set; }
    }
}