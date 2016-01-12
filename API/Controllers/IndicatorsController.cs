using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using HarvestChoiceApi.Models;
using HarvestChoiceApi.Documentation.Models;
using System.Web.Http.OData.Query;

namespace HarvestChoiceApi.Controllers
{
    /// <summary>
    /// API class for accessing indicators
    /// </summary>
    public class IndicatorsController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();

        // GET api/Indicator
        /// <summary>
        /// Method for getting all indicators. 
        /// </summary>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/indicators</example>
        /// <returns>A list of all indicators.</returns>
        [Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
        [ApiReturnType(typeof(Indicator))]
        public IEnumerable<Indicator> Getindicator_metadata()
        {
            var result = (from i in db.indicator_metadata
                          where i.published == true && i.genRaster == true
                          select new {
                              i.ID,
                              i.varCode,
                              i.varLabel,
                              i.varTitle,
                              i.unit,
                              i.year,
                              i.yearEnd,
                              i.dec,
                              i.type,
                              i.aggType,
                              i.aggFun,
                              i.varDesc,
                              i.sources,
                              i.owner,
                              i.isDomain
                          }).ToList();

            List<Indicator> indicators = result.AsEnumerable()
                .Select(o => new Indicator
                {
                    Id = o.ID,
                    ColumnName = o.varCode,
                    MicroLabel = o.varLabel,
                    ShortLabel = o.varLabel,
                    FullLabel = o.varTitle,
                    Unit = o.unit,
                    Year = o.year,
                    EndYear = o.yearEnd,
                    DecimalPlaces = o.dec,
                    ClassificationType = o.type,
                    AggType = o.aggType,
                    AggFormula = o.aggFun,
                    LongDescription = o.varDesc,
                    Source = o.sources,
                    DataWorker = o.owner,
                    IsDomain = (bool)o.isDomain
                }).ToList();
            
            return indicators;
        }

        // GET api/Indicator/5
        /// <summary>
        /// Method of obtaining indicators by id.
        /// </summary>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/indicators/11</example>
        /// <param name="id">Unique indicator identifier.</param>
        /// <returns>A single indicator.</returns>
        [ApiReturnType(typeof(Indicator))]
        public Indicator Getindicator_metadata(int id)
        {
            indicator_metadata indicator_metadata = db.indicator_metadata.Find(id);
            if (indicator_metadata == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            Indicator indicator = new Indicator();
            indicator.Id = indicator_metadata.ID;
            indicator.ColumnName = indicator_metadata.varCode;
            indicator.MicroLabel = indicator_metadata.varLabel;
            indicator.ShortLabel = indicator_metadata.varLabel;
            indicator.FullLabel = indicator_metadata.varTitle;
            indicator.Unit = indicator_metadata.unit;
            indicator.Year = indicator_metadata.year;
            indicator.EndYear = indicator_metadata.yearEnd;
            indicator.DecimalPlaces = indicator_metadata.dec;
            indicator.ClassificationType = indicator_metadata.type;
            indicator.AggType = indicator_metadata.aggType;
            indicator.AggFormula = indicator_metadata.aggFun;
            indicator.LongDescription = indicator_metadata.varDesc;
            indicator.Source = indicator_metadata.sources;
            indicator.DataWorker = indicator_metadata.owner;
            indicator.IsDomain = (bool)indicator_metadata.isDomain;

            return indicator;
        }
       
    }
}