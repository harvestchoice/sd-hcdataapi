using HarvestChoiceApi.Documentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using HarvestChoiceApi.Areas.HelpPage;

namespace HarvestChoiceApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //removes xml as the default data type causing JSON to be default
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            //enables odata query capabilities globally
            //config.EnableQuerySupport();

            // Enable CORS
            config.EnableCors();

            //add the xml documentation file to the documentation provider
            List<string> xmlDocumentPath = new List<string>();
            xmlDocumentPath.Add("~/bin/HarvestChoiceApi.XML");

            config.Services.Replace(typeof(IDocumentationProvider),
               new MultipleXmlDocumentationProvider(xmlDocumentPath.ToArray()));

        }
    }
}
