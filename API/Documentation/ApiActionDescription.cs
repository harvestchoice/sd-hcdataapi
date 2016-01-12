namespace HarvestChoiceApi.Documentation.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Description;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Documentation for an <see cref="ApiController"/> action
    /// </summary>
    public class ApiActionDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiActionDescription" /> class.
        /// </summary>
        public ApiActionDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiActionDescription" /> class.
        /// </summary>
        /// <param name="descriptions">The description.</param>
        public ApiActionDescription(IEnumerable<ApiDescription> descriptions)
        {
            var description = descriptions.First();

            this.Name = description.ActionDescriptor.ActionName;
            this.ParameterDescriptions = description.ParameterDescriptions;
            this.Routes = descriptions.Select(x => new ApiRouteDescription(x));
            this.MainRoute = string.Format("{0} {1}", this.Routes.First().Method, this.Routes.First().Path);

            try
            {
                this.Summary = JsonConvert.DeserializeObject<JObject>(description.Documentation).Value<string>("summary");
                this.Example = JsonConvert.DeserializeObject<JObject>(description.Documentation).Value<string>("example");
                this.Remarks = JsonConvert.DeserializeObject<JObject>(description.Documentation).Value<string>("remarks");
                this.Returns = JsonConvert.DeserializeObject<JObject>(description.Documentation).Value<string>("returns");
            }
            catch (JsonReaderException jrException)
            {
                this.Summary = string.Empty;
                this.Example = string.Empty;
                this.Remarks = string.Empty;
                this.Returns = string.Empty;
            }

            // Generate sample requests
            var sampleRequests = new List<ApiActionSample>();

            foreach (var mediaTypeFormatter in description.SupportedRequestBodyFormatters)
            {
                var request = new ApiActionSample(mediaTypeFormatter, description, ApiActionSampleDirection.Request);

                if (request.Sample != null)
                {
                    sampleRequests.Add(request);
                }
            }

            this.SampleRequests = sampleRequests;

            // Generate sample responses
            var sampleResponses = new List<ApiActionSample>();

            foreach (var mediaTypeFormatter in description.SupportedResponseFormatters)
            {
                var response = new ApiActionSample(mediaTypeFormatter, description, ApiActionSampleDirection.Response);

                if (response.Sample != null)
                {
                    sampleResponses.Add(response);
                }
            }

            this.SampleResponses = sampleResponses;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiActionDescription" /> class.
        /// </summary>
        /// <param name="group">The group.</param>
        public ApiActionDescription(IGrouping<string, ApiDescription> group)
        {
            this.Name = group.First().ActionDescriptor.ActionName;
            this.ParameterDescriptions = group.Select(a => a.ParameterDescriptions).First();
            this.Routes = group.Select(a => new ApiRouteDescription(a));

            try
            {
                this.Summary = JsonConvert.DeserializeObject<JObject>(group.Key).Value<string>("summary");
                this.Example = JsonConvert.DeserializeObject<JObject>(group.Key).Value<string>("example");
                this.Remarks = JsonConvert.DeserializeObject<JObject>(group.Key).Value<string>("remarks");
                this.Returns = JsonConvert.DeserializeObject<JObject>(group.Key).Value<string>("returns");
            }
            catch (JsonReaderException jrException)
            {
                this.Summary = string.Empty;
                this.Example = string.Empty;
                this.Remarks = string.Empty;
                this.Returns = string.Empty;
            }

            // Generate sample requests
            var sampleRequests = new List<ApiActionSample>();

            foreach (var mediaTypeFormatter in group.First().SupportedRequestBodyFormatters)
            {
                var request = new ApiActionSample(mediaTypeFormatter, group.First(), ApiActionSampleDirection.Request);

                if (request.Sample != null)
                {
                    sampleRequests.Add(request);
                }
            }

            this.SampleRequests = sampleRequests;

            // Generate sample responses
            var sampleResponses = new List<ApiActionSample>();

            foreach (var mediaTypeFormatter in group.First().SupportedResponseFormatters)
            {
                var response = new ApiActionSample(mediaTypeFormatter, group.First(), ApiActionSampleDirection.Response);

                if (response.Sample != null)
                {
                    sampleResponses.Add(response);
                }
            }

            this.SampleResponses = sampleResponses;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the paths.
        /// </summary>
        /// <value>
        /// The paths.
        /// </value>
        public IEnumerable<ApiRouteDescription> Routes { get; set; }

        /// <summary>
        /// Gets or sets the main route.
        /// </summary>
        /// <value>The main route.</value>
        public string MainRoute { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the example.
        /// </summary>
        /// <value>
        /// The example.
        /// </value>
        public string Example { get; set; }

        /// <summary>
        /// Gets or sets the remarks.
        /// </summary>
        /// <value>
        /// The remarks.
        /// </value>
        public string Remarks { get; set; }

        /// <summary>
        /// Gets or sets the returns.
        /// </summary>
        /// <value>
        /// The returns.
        /// </value>
        public string Returns { get; set; }

        /// <summary>
        /// Gets or sets the sample requests.
        /// </summary>
        /// <value>The sample requests.</value>
        public IEnumerable<ApiActionSample> SampleRequests { get; set; }

        /// <summary>
        /// Gets or sets the sample responses.
        /// </summary>
        /// <value>The sample responses.</value>
        public IEnumerable<ApiActionSample> SampleResponses { get; set; }

        /// <summary>
        /// Gets or sets the parameter descriptions.
        /// </summary>
        /// <value>
        /// The parameter descriptions.
        /// </value>
        public IEnumerable<ApiParameterDescription> ParameterDescriptions { get; set; }
    }
}