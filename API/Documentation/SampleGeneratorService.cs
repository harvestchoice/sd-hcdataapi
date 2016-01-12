namespace HarvestChoiceApi.Documentation.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    using Newtonsoft.Json;

    /// <summary>
    /// Ther sample generator service
    /// </summary>
    public class SampleGeneratorService
    {
        /// <summary>
        /// The singleton
        /// </summary>
        private static readonly SampleGeneratorService Singleton = new SampleGeneratorService();

        /// <summary>
        /// Prevents a default instance of the <see cref="SampleGeneratorService" /> class from being created.
        /// </summary>
        private SampleGeneratorService()
        {
            this.SampleObjects = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the objects that are serialized as samples by the supported formatters.
        /// </summary>
        public IDictionary<Type, object> SampleObjects { get; internal set; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SampleGeneratorService Instance
        {
            get
            {
                return Singleton;
            }
        }

        /// <summary>
        /// Gets the sample object that will be serialized by the formatters. 
        /// First, it will look at the <see cref="SampleObjects"/>. If no sample object is found, it will try to create one using <see cref="ObjectGenerator"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The sample object.</returns>
        public virtual object GetSampleObject(Type type)
        {
            object sampleObject;

            if (!SampleObjects.TryGetValue(type, out sampleObject))
            {
                // Try create a default sample object
                var objectGenerator = new ObjectGenerator();

                sampleObject = objectGenerator.GenerateObject(type);

                // Add sample object to dictionary for possible future use
                this.SampleObjects.Add(type, sampleObject);
            }

            return sampleObject;
        }

        /// <summary>
        /// Tries to format the string as indented json.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Indented json.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        public string TryFormatJson(string str)
        {
            try
            {
                object parsedJson = JsonConvert.DeserializeObject(str);
                return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            }
            catch
            {
                // can't parse JSON, return the original string
                return str;
            }
        }

        /// <summary>
        /// Tries to format the string as indented XML.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Indented XML.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Handling the failure by returning the original string.")]
        public string TryFormatXml(string str)
        {
            try
            {
                XDocument xml = XDocument.Parse(str);
                return xml.ToString();
            }
            catch
            {
                // can't parse XML, return the original string
                return str;
            }
        }
    }
}