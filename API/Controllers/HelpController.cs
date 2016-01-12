namespace HarvestChoiceApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Mvc;
    using AttributeRouting.Framework;
    using HarvestChoiceApi.Documentation;
    using HarvestChoiceApi.Documentation.Interfaces;
    using HarvestChoiceApi.Documentation.Models;
    using HarvestChoiceApi.Documentation.ViewModels;

    /// <summary>
    /// Controller for viewing automatically generated documentation.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// The controllers.
        /// </summary>
        private readonly IList<ApiControllerDescription> controllers;

        /// <summary>
        /// The current assembly.
        /// </summary>
        private Assembly currentAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController" /> class.
        /// </summary>
        public HelpController()
        {
            this.controllers = GlobalConfiguration.Configuration.Services.GetApiExplorer().ApiDescriptions.Select(
                                    x => new ApiControllerDescription()
                                    {
                                        Name = x.ActionDescriptor.ControllerDescriptor.ControllerName
                                    }).ToList();
        }

        /// <summary>
        /// Default view.
        /// </summary>
        /// <returns>The default view.</returns>
        public ActionResult Index()
        {
            var viewmodel = new ApiExplorerViewModel()
                {
                    ApiControllerDescriptions = this.controllers,
                    CurrentAssembly = this.currentAssembly ?? (this.currentAssembly = Assembly.GetAssembly(HttpContext.ApplicationInstance.GetType().BaseType))
                };

            return this.View(viewmodel);
        }

        /// <summary>
        /// Details view
        /// </summary>
        /// <param name="id">The controller name.</param>
        /// <returns>The detailed view of a Controller.</returns>
        public ActionResult Details(string id)
        {
            // Get actions
            var actions =
                GlobalConfiguration.Configuration.Services.GetApiExplorer().ApiDescriptions
                    .GroupBy(x => x.ActionDescriptor.ControllerDescriptor.ControllerName)
                    .First(x => x.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            var controller = new ApiControllerDescription()
                {
                    Name = actions.First().ActionDescriptor.ControllerDescriptor.ControllerName,
                    Actions = actions.GroupBy(x => new { x.ActionDescriptor.ActionName, ConcatParameterNames = string.Join(",", x.ParameterDescriptions.Select(p => p.Name)) }).Select(x => new ApiActionDescription(x)).OrderBy(a => a.Name).ToList()
                };
            if (id == "CellValues")
            {
                controller.Actions[1].Example = "CellValuesExample";
            }

            var viewmodel = new ApiExplorerDetailsViewModel()
                           {
                               ApiControllerDescriptions = this.controllers,
                               CurrentAssembly = this.currentAssembly ?? (this.currentAssembly = Assembly.GetAssembly(HttpContext.ApplicationInstance.GetType().BaseType)),
                               ApiControllerDescription = controller
                           };

            return View(viewmodel);
        }        

        /// <summary>
        /// Models diagram view.
        /// </summary>
        /// <returns>The model diagram view.</returns>
        public ActionResult ModelDiagram()
        {
            var modelDiagramProvider = this.GetModelDiagramProvider();

            var viewmodel = new ModelDiagramViewModel()
            {
                ApiControllerDescriptions = this.controllers,
                CurrentAssembly = this.currentAssembly ?? (this.currentAssembly = Assembly.GetAssembly(HttpContext.ApplicationInstance.GetType().BaseType)),
                ModelDiagramPath = modelDiagramProvider.Image
            };

            return this.View(viewmodel);
        }

        /// <summary>
        /// Gets the model diagram provider.
        /// </summary>
        /// <returns>The current model diagram provider. Returns the default implementation if none is registered.</returns>
        private IModelDiagramProvider GetModelDiagramProvider()
        {
            var service = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IModelDiagramProvider)) as IModelDiagramProvider;

            return service ?? new YumlModelDiagramProvider();
        }

        public ActionResult CellValuesExample()
        {
            var viewmodel = new ApiExplorerViewModel()
            {
                ApiControllerDescriptions = this.controllers,
                CurrentAssembly = this.currentAssembly ?? (this.currentAssembly = Assembly.GetAssembly(HttpContext.ApplicationInstance.GetType().BaseType))
            };

            return this.View(viewmodel);
        }
    }
}
