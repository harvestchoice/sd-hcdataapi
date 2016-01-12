namespace HarvestChoiceApi.Documentation.Models
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http.Description;

    using HarvestChoiceApi.Documentation.Functions;

    /// <summary>
    /// Sample request/response for api actions
    /// </summary>
    public class ApiActionSample
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiActionSample" /> class.
        /// </summary>
        public ApiActionSample()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiActionSample" /> class.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="apiDescription">The API description.</param>
        /// <param name="sampleDirection">The sample direction.</param>
        public ApiActionSample(
            MediaTypeFormatter formatter, ApiDescription apiDescription, ApiActionSampleDirection sampleDirection)
        {
            Type type = null;

            if (formatter != null)
            {
                this.MediaType = formatter.SupportedMediaTypes.First().MediaType;

                if (sampleDirection == ApiActionSampleDirection.Request)
                {
                    var requestBodyParameter =
                        apiDescription.ParameterDescriptions.FirstOrDefault(
                            p => p.Source == ApiParameterSource.FromBody);

                    type = requestBodyParameter == null
                                            ? null
                                            : requestBodyParameter.ParameterDescriptor.ParameterType;
                }
                else if (sampleDirection == ApiActionSampleDirection.Response)
                {
                    var returnTypes = apiDescription.ActionDescriptor.GetCustomAttributes<ApiReturnTypeAttribute>();

                    if (returnTypes.Any())
                    {
                        type = returnTypes.First().ReturnType;
                    }
                }
            }

            if (type != null && formatter.CanWriteType(type))
            {
                var content =
                            new ObjectContent(
                                type,
                                SampleGeneratorService.Instance.GetSampleObject(type),
                                formatter).ReadAsStringAsync().Result;

                if (this.MediaType.ToUpperInvariant().Contains("XML"))
                {
                    this.Sample = SampleGeneratorService.Instance.TryFormatXml(content);
                }
                else if (this.MediaType.ToUpperInvariant().Contains("JSON"))
                {
                    this.Sample = SampleGeneratorService.Instance.TryFormatJson(content);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>The type of the media.</value>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the sample.
        /// </summary>
        /// <value>The sample.</value>
        public object Sample { get; set; }
    }
}