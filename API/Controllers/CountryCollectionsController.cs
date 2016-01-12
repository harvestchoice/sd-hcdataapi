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
    public class CountryCollectionsController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();

        /// <summary>
        /// Gets all collections of countries. 
        /// </summary>
        /// <remarks>Countries are grouped into collections. The collections have names (name), identifiers (code), categories (group) and a listing of country identifiers (ISOs). For more information on countries see the Countries method. </remarks>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/countrycollections</example>
        /// <returns>A list of country collections.</returns>
        [Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
        [ApiReturnType(typeof(CountryCollection))]
        public IEnumerable<CountryCollection> GetCountryCollections()
        {
            List<CountryCollection> CountryLists = new List<CountryCollection>();

            IEnumerable<collection> collections = db.collections;
            
            foreach (collection collection in collections)
            {
                //cast the collection group to a list
                IEnumerable<collection_group> groups = collection.collection_group;

                //we only are sending collections that have at least one group
                if (groups.Any())
                {
                    //create a collection for each group
                    foreach (collection_group group in groups)
                    {
                        CountryCollection countryColllection = new CountryCollection();
                        countryColllection.name = collection.name;
                        countryColllection.label = collection.label;
                        countryColllection.code = collection.code;
                        countryColllection.group = group.group;

                        IEnumerable<country_collection> countries = collection.country_collection.OrderBy(x => x.country.name);
                        int index = 0;
                        countryColllection.ISOs = new string[countries.Count()];

                        foreach (country_collection country in countries)
                        {
                            countryColllection.ISOs[index] = country.country_id;
                            index++;
                        }

                        CountryLists.Add(countryColllection);
                    }
                }
            }
            return CountryLists;


        }
    }
}
