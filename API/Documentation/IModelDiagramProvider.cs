namespace HarvestChoiceApi.Documentation.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The model diagram provider interface
    /// </summary>
    public interface IModelDiagramProvider
    {
        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>The models.</value>
        IEnumerable<Type> Models { get; }

        /// <summary>
        /// Gets the path to the model diagram image
        /// </summary>
        /// <value>The model diagram image.</value>
        string Image { get; }
    }
}