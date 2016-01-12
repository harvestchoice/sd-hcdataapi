using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HarvestChoiceApi.Models;
using System.Web.Http.OData.Query;
using HarvestChoiceApi.Documentation.Models;

namespace HarvestChoiceApi.Controllers
{
    public class CountriesController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();

        /// <summary>
        /// Gets all countries within the Harvest Choice area of interest.
        /// </summary>
        /// <remarks></remarks>
        /// <example>http://dev.harvestchoice.org/harvestchoiceapi/0.1/api/countries</example>
        /// <returns>A list of country names and identifiers.</returns>
        [Queryable(AllowedQueryOptions =  AllowedQueryOptions.All,
            AllowedFunctions = AllowedFunctions.All)]
        [ApiReturnType(typeof(vCountryList))]
        public IQueryable<vCountryList> GetCountries()
        {
            return db.vCountryLists;
        }
       
    }
}
