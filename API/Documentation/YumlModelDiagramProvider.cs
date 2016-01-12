namespace HarvestChoiceApi.Documentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;

    using HarvestChoiceApi.Documentation.Interfaces;

    using Yuml.Net;
    using Yuml.Net.Interfaces;

    /// <summary>
    /// A Yuml-based model diagram provider
    /// </summary>
    public class YumlModelDiagramProvider : IModelDiagramProvider
    {
        /// <summary>
        /// The yuml generator
        /// </summary>
        private readonly IYumlFactory yumlFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="YumlModelDiagramProvider" /> class.
        /// </summary>
        public YumlModelDiagramProvider()
        {
            // Get all models from calling assembly
            this.Models = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsPublic && x.IsVisible && !x.IsSpecialName && !x.IsAbstract).ToList();

            // Configure generator
            this.yumlFactory = this.GetYumlFactory(this.Models);

            // Get image path
            this.Image = this.yumlFactory.GenerateClassDiagramUri().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YumlModelDiagramProvider" /> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public YumlModelDiagramProvider(IEnumerable<Type> models)
        {
            this.Models = models;

            // Configure generator
            this.yumlFactory = this.GetYumlFactory(this.Models);

            // Get image path
            this.Image = this.yumlFactory.GenerateClassDiagramUri().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YumlModelDiagramProvider" /> class.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="detailLevels">The detail levels.</param>
        public YumlModelDiagramProvider(IEnumerable<Type> models, DetailLevel[] detailLevels)
        {
            this.Models = models;

            // Configure generator
            this.yumlFactory = this.GetYumlFactory(this.Models);

            // Get image path
            this.Image = this.yumlFactory.GenerateClassDiagramUri(detailLevels).ToString();
        }

        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>The models.</value>
        public IEnumerable<Type> Models { get; private set; }

        /// <summary>
        /// Gets the path to the model diagram image
        /// </summary>
        /// <value>The model diagram image.</value>
        public string Image { get; private set; }

        /// <summary>
        /// Gets the yUML factory.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns>The registered type for IYumlFactory, otherwise returns the default implementation.</returns>
        private IYumlFactory GetYumlFactory(IEnumerable<Type> models)
        {
            var service = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IYumlFactory)) as IYumlFactory;

            return service ?? new YumlFactory(models.ToList());
        }
    }
}