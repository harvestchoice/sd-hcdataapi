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
    public class RegionsController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HC_Entities();
              
        /// <summary>
        /// Gets all Region Names (GAUL 1) within the Harvest Choice area of interest.
        /// </summary>
        /// <remarks></remarks>
        /// <example>http://dev.harvestchoice.org/harvestchoiceapi/0.1/api/regions</example>
        /// <returns>A list of region names.</returns>
        [Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
        [ApiReturnType(typeof(Region))]
        public IEnumerable<Region> GetRegions()
        {
            var results = (from r in db.GAUL_2008_1
                select new { r.GAUL_2008_11 }).Distinct().ToList();

            List<Region> region = new List<Region>();

            region = results.AsEnumerable().OrderBy(o => o.GAUL_2008_11).Select( o => new Region
            {
                Name = o.GAUL_2008_11
            }).ToList();

            return region.AsEnumerable();
        }
    }
}
