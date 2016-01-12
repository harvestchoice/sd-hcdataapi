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
using System.Web.Http.OData.Query;
using HarvestChoiceApi.Documentation.Models;

namespace HarvestChoiceApi.Controllers
{
    public class DomainsController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();

        /// <summary>
        /// Gets all domains. 
        /// </summary>
        /// <remarks>Domains are spatial layers or groupings of spatial layers that contain various data within the Harvest Choice area of interest. Domains are used to aggregate the grid cell data. For more information on the grid cell data see the CellValues method.</remarks>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/domains</example>
        /// <returns>A list of all domains.</returns>
        // GET api/Domains
        [Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
        [ApiReturnType(typeof(Domains))]
        public IEnumerable<Domains> Getschemata()
        {
            var results =  (from s in db.schemata
                where (s.published == true || s.published == null) 
                && (s.for_mappr == true || s.for_mappr == null)
                select new {
                    s.id,
                    s.description,
                    s.name,
                    s.schema_domain
                }).ToList();

            List<Domains> domains = new List<Domains>();
            domains = results.AsEnumerable()
                 .Select(o => new Domains
                 {
                     Id = o.id,
                     Description = o.description,
                     Name = o.name,
                     DomainAreas = o.schema_domain.AsEnumerable()
                        .Select(x => new DomainArea
                        { Id = x.domainid,
                            ColumnName = x.domain_variable.column_name,
                            MicroLabel = x.domain_variable.micro_label,
                            LongDescription = x.domain_variable.long_description,
                            Unit = x.domain_variable.unit,
                            Year = x.domain_variable.year,
                            DecimalPlaces = x.domain_variable.decimal_places,
                            ClassificationType = x.domain_variable.classification_type,
                            AggType = x.domain_variable.agg_type,
                            AggFormula = x.domain_variable.agg_formula}).ToList()
                 }).ToList();

            return domains;
        }

        /// <summary>
        /// Gets a single domain by its identifier. 
        /// </summary>
        /// <remarks>Domains are spatial layers or groupings of spatial layers that contain various data within the Harvest Choice area of interest. Domains are used to aggregate the grid cell data. For more information on the grid cell data see the CellValues method.</remarks>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/domains</example>
        /// <param name="id">Unique indicator identifier.</param>
        /// <returns>A domain.</returns>
        // GET api/Domains/5
        [ApiReturnType(typeof(Domains))]
        public Domains Getschema(int id)
        {

            schema schema = db.schemata.Find(id);


            if (schema == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            Domains domain = new Domains();
            domain.Id = schema.id;
            domain.Name = schema.name;
            domain.Description = schema.description;
            domain.DomainAreas = schema.schema_domain.AsEnumerable()
                .Select(x => new DomainArea
                        { Id = x.domainid,
                            ColumnName = x.domain_variable.column_name,
                            MicroLabel = x.domain_variable.micro_label,
                            LongDescription = x.domain_variable.long_description,
                            Unit = x.domain_variable.unit,
                            Year = x.domain_variable.year,
                            DecimalPlaces = x.domain_variable.decimal_places,
                            ClassificationType = x.domain_variable.classification_type,
                            AggType = x.domain_variable.agg_type,
                            AggFormula = x.domain_variable.agg_formula}).ToList();

            return domain;
        }

    }
}