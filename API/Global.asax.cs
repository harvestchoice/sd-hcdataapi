using HarvestChoiceApi.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
//using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApiContrib.Formatting.Jsonp;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Http.Cors.WebApi;
using Thinktecture.IdentityModel.Http.Cors.IIS;


namespace HarvestChoiceApi
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Add JSONP formatting to the global formatting collection
            GlobalConfiguration.Configuration.AddJsonpFormatter();

            //Add CORS handling
            //GlobalConfiguration.Configuration.MessageHandlers.Add(
            //    new HarvestChoiceApi.Classes.CorsHandler());

            //CorsConfig.RegisterCors(UrlBasedCorsConfiguration.Configuration);
            CorsConfig.RegisterCors(GlobalConfiguration.Configuration);
        }


    }

    public class CorsConfig
    {
        public static void RegisterCors(HttpConfiguration httpConfig)
        {

            WebApiCorsConfiguration corsConfig = new WebApiCorsConfiguration();

            corsConfig.RegisterGlobal(httpConfig);
            corsConfig.AllowAll();

        }
    }
}