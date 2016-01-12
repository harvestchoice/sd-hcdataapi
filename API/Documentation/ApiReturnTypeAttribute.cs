namespace HarvestChoiceApi.Documentation.Models
{
    using System;

    /// <summary>
    /// Attribute for setting the action return type for the documentation provider
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiReturnTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiReturnTypeAttribute" /> class.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        public ApiReturnTypeAttribute(Type returnType)
        {
            this.ReturnType = returnType;
        }

        /// <summary>
        /// Gets or sets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ReturnType { get; set; }
    }
}