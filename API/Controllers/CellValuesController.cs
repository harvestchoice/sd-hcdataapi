using HarvestChoiceApi.Documentation.Models;
using HarvestChoiceApi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Spatial;
using System.Text;
using System.Collections;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using ESRI.ArcGIS.Geometry;
using System.Data;


namespace HarvestChoiceApi.Controllers
{
    public class CellValuesController : ApiController
    {
        //EF Connection
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();
        //Database connection
        private SqlConnection SQLConnection =
                new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["HC_WEB_DB_2"].ConnectionString);

        //ArcGIS Geoprocessing Service
        string gpURL = "http://dev.harvestchoice.org/ArcGIS/rest/services/GPService/TravelTime/GPServer/TravelTimeZones";

        //flag if using country boundries as a domain
        private bool usingISO3 = false;

        //flag if using gaul boundries as a domain
        private bool usingGAUL = false;
   
        // GET api/CellValues/5
        /// <summary>
        /// Gets all data for a specific cell value.
        /// </summary>
        /// <param name="id">CELL5M Id.</param>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/cellvalues/3267978</example>
        /// <returns>All data for a specific cell value.</returns>
        [ApiReturnType(typeof(CELL_VALUES))]
        public CELL_VALUES GetCELL_VALUES(int id)
        {
            CELL_VALUES cell_values = db.CELL_VALUES.Find(id);
            if (cell_values == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return cell_values;
        }

        /// <summary>
        /// Gets requested indicator information by aggregating cell values according to provided domains and countries. (SAME as POST)
        /// </summary>
        /// <example>CellValuesExample</example>
        /// <param name="indicatorIds">Required. List of indicator ids. See Indicators for more information.</param>
        /// <param name="domainIds">Optional. List of domain ids. See Domains for more information.</param>
        /// <param name="countryIds">Optional. List of country ids. See Countries for more information. CountryCollections contain lists of country ids in the ISOs field.</param>
        /// <param name="wktGeometry">Optional. Well-known text geometry using web mercator projection.</param>
        /// <param name="pivotDomainId">Optional. A single domain id. Used to pivot the data by domain. </param>
        /// <param name="returnGeometry">Optional. True or False, to return geometry results (as well-known text) along with tablular data. Only available if pivotDomainId and wktGeometry are null.</param>
        /// <param name="groupCountry">Optional. True or False, will group tabular data by country. Must use countryIds parameter. Only available if pivotDomainId and wktGeometry are null.</param>
        /// <param name="doMarketAnalysis">Optional. True or False, will conduct a 2,4,6,8 hr market shed analysis for a given point and return tabular data for indicator within each polygon. </param>
        /// <param name="regionNames">Required only when using domain 'GAUL Region and District Boundaries'. See Domains for more information on domains. List of region names (maximum of three allowed). See Regions for more information.</param>
        /// <returns>Returns a table of indicators based on cell values aggregated on the provided domain and limited by the provided countries. ColumnList describes each column. ValueList contains the table rows.</returns>
        [ApiReturnType(typeof(CellValues))]
        [HttpGet]
        public IHttpActionResult GetCellValues([FromUri] int[] indicatorIds = null, [FromUri]int[] domainIds = null,
        [FromUri]string[] countryIds = null, [FromUri]string wktGeometry = null, [FromUri]int pivotDomainId = 0,
            [FromUri]bool returnGeometry = false, [FromUri]bool groupCountry = false, [FromUri]bool doMarketAnalysis = false,
            [FromUri]string[] regionNames = null)
        {            
            if (indicatorIds != null || wktGeometry != null)
            {
                #region Declarations
                CellValues cellvalues = new CellValues();//returned object

                List<Indicator> indicators = new List<Indicator>();//list of valid indicators
                List<Domains> domains = new List<Domains>();//list of valid domains (used to aggregate rows)
                List<country> countries = new List<country>();//list of valid countries
                List<CountryRegion> countryRegions = new List<CountryRegion>(); //list of valid region/district names
                Domains pivotDomain = new Domains(); //valid pivot domain (used to aggregate columns)
                List<string> columnList = new List<string>();//holds gathered list of required columns
                List<string> selectList = new List<string>();//holds gathered list of select statements
                List<string> whereList = new List<string>();//holds gathered list of where statements
                List<string> groupList = new List<string>();//holds gathered list of group statements
                List<string> orderList = new List<string>();//holds gathered list of order statements
                List<List<string>> selectMatrix = new List<List<string>>();//for preparing pivot columns
                Columns column = new Columns();
                string select = "SELECT "; //select string for building the sql statement
                string from = string.Empty;//select string for building the sql statement
                string where = " WHERE "; //where string for building the sql statement
                string groupby = " GROUP BY "; //groupby string for building the sql statement
                string orderby = " ORDER BY "; //orderby string for building the sql statement
                string sqlstatement = string.Empty;
                int columnIndex = 0;
                usingISO3 = false;
                bool requiresGeometry = false;
                HttpStatusCode httpStatus = HttpStatusCode.OK; // track success of execution
                #endregion

                try
                {

                    #region Validate Parameters
                    //Validate indicator ids
                    if (indicatorIds != null && indicatorIds.Length != 0)
                        indicators = ValidateIndicators(indicatorIds);

                    //Validate region names
                    if (regionNames != null && regionNames.Length != 0)
                        countryRegions = ValidateRegions(regionNames);
                        //regions = ValidateRegions(regionNames);

                    //Validate domain ids
                    if (domainIds != null && domainIds.Length != 0)
                        domains = ValidateDomains(domainIds);

                    //Validate pivot domain id
                    if (pivotDomainId > 0)
                        pivotDomain = ValidatePivotDomain(pivotDomainId);

                    //Validate country ids
                    if (countryIds != null && countryIds.Length != 0)
                        countries = ValidateCountries(countryIds);

                    #endregion

                    //Must have at least 1 indicator
                    if (indicators.Count > 0)
                    {

                        //if there are ONLY ONE indicator and no other parameters 
                        //give them raw data (no more than one indicator - its too expensive)
                        #region Single Indicator (no other parameters)
                        if (indicators.Count == 1 && domains.Count == 0 && pivotDomainId == 0
                            && countries.Count == 0 && string.IsNullOrEmpty(wktGeometry))
                        {
                            //add a couple standard columns (Cell5M & ISO3)
                            //capture column information   
                            column = new Columns();
                            column.ColumnName = "CELL5M";
                            column.ColumnIndex = columnIndex.ToString();
                            column.ColumnDataType = "System.Int32";
                            columnIndex++; //increment the column index                       
                            cellvalues.ColumnList.Add(column);

                            selectList.Add("CELL5M");
                            columnList.Add("CELL5M");

                            //capture column information   
                            column = new Columns();
                            column.ColumnName = "ISO3";
                            column.ColumnIndex = columnIndex.ToString();
                            column.ColumnDataType = "System.String";
                            columnIndex++; //increment the column index                       
                            cellvalues.ColumnList.Add(column);

                            selectList.Add("ISO3");
                            columnList.Add("ISO3");

                            //loop through the indicators
                            foreach (Indicator indicator in indicators)
                            {
                                //capture column information   
                                column = new Columns();
                                column.ColumnName = indicator.ColumnName;
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                columnIndex++; //increment the column index                       
                                cellvalues.ColumnList.Add(column);

                                selectList.Add(indicator.ColumnName);
                                columnList.Add(indicator.ColumnName);
                            }
                        }
                        #endregion

                        else
                        {
                            //if we are doing a market analysis set the first column 
                            //as Travel Time 
                            if (doMarketAnalysis)
                            {
                                //capture column information 
                                column = new Columns();
                                column.ColumnName = "Travel Time";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.String";
                                columnIndex++; //increment the column index                       
                                cellvalues.ColumnList.Add(column);
                            }

                            //has domain request & no geometry
                            #region Process domain select,where & groupby statements
                            if (domains.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                            {
                                foreach (Domains domain in domains)
                                {
                                    //if using gual boundries as a domain, MUST also have at least one valid region
                                    if ((domain.Id == 37 && countryRegions.Count > 0) || (domain.Id != 37))
                                    {

                                        foreach (DomainArea domainArea in domain.DomainAreas)
                                        {
                                            string preparedSelect = string.Empty;
                                            string preparedSort = string.Empty;
                                            List<string> preparedWhere = new List<string>();
                                            bool sorted = false;
                                            bool orderMe = false;

                                            #region GAUL Region and District Boundaries
                                            //This domain is too large to add all the classifications to the statment
                                            if (domain.Id == 37)
                                            {
                                                //GAUL Region
                                                if (domainArea.Id == 31)
                                                {
                                                    List<string> names = countryRegions.Select(x => x.Name).ToList();
                                                    domainArea.Classifications.RemoveAll(x => !names.Contains(x.Name));
                                                }
                                                ////GAUL District
                                                //if (domainArea.Id == 32)
                                                //{
                                                //    List<string> names = (from b in regions from a in b.Districts select a.Name).Distinct().ToList();
                                                //    domainArea.Classifications.RemoveAll(x => !names.Contains(x.Name));
                                                //}
                                            }
                                            #endregion

                                            #region Prepare the inner statement logic
                                            if (domainArea.ClassificationType.ToUpper() == "CONTINUOUS")
                                            {
                                                foreach (Classification classification in domainArea.Classifications)
                                                {
                                                    string statement = domainArea.ColumnName + " >= " + classification.Min +
                                                        " AND " + domainArea.ColumnName + " <= " + classification.Max;

                                                    preparedSelect += " WHEN " + statement + " THEN '" + classification.Name + "'";

                                                    if (classification.SortOrder != 0)
                                                    {
                                                        preparedSort += " WHEN " + statement + " THEN '" + classification.SortOrder + "'";
                                                        sorted = true;
                                                    }
                                                    preparedWhere.Add(statement);
                                                }
                                            }
                                            else if (domainArea.ClassificationType.ToUpper() == "CLASS")
                                            {
                                                foreach (Classification classification in domainArea.Classifications)
                                                {
                                                    int num = 0;
                                                    bool isValueNumeric = int.TryParse(classification.Value, out num);
                                                    string statement = string.Empty;

                                                    statement = isValueNumeric ? domainArea.ColumnName + " = " + classification.Value :
                                                        domainArea.ColumnName + " = '" + fixSingleQuote(classification.Value) + "'";

                                                    preparedSelect += " WHEN " + statement + " THEN '" + fixSingleQuote(classification.Name) + "'";

                                                    if (classification.SortOrder != 0 && classification.SortOrder != null)
                                                    {
                                                        preparedSort += " WHEN " + statement + " THEN '" + classification.SortOrder + "'";
                                                        sorted = true;
                                                    }
                                                    else
                                                    {
                                                        orderMe = true; //set this column in the orderby statement
                                                    }

                                                    preparedWhere.Add(statement);
                                                }
                                            }
                                            #endregion

                                            #region Assign select, groupby and where statements for domain column
                                            if (!string.IsNullOrEmpty(preparedSelect) && preparedWhere.Count > 0
                                                && preparedWhere != null)
                                            {
                                                //Domain column statements
                                                selectList.Add("CASE" + preparedSelect + " END AS '" + domainArea.ColumnName + "'");
                                                groupList.Add("CASE" + preparedSelect + "END");
                                                whereList.Add("(" + String.Join(" OR ", preparedWhere.ToArray()) + ")");

                                                //capture column information 
                                                column = new Columns(domainArea.ColumnName, columnIndex.ToString());
                                                column.ColumnDataType = GetColumnDataType(domainArea.ColumnName);
                                                columnIndex++; //increment the column index                       
                                                if (sorted) column.SortColumnIndex = columnIndex.ToString();

                                                columnList.Add(domainArea.ColumnName);

                                                //add the column information to the return type
                                                cellvalues.ColumnList.Add(column);

                                            }
                                            #endregion

                                            #region Assign select, groupby and order statements for domain sort column
                                            if (sorted && !string.IsNullOrEmpty(preparedSort))
                                            {
                                                selectList.Add("CASE" + preparedSort + " END AS 'sortorder_" + domainArea.ColumnName + "'");
                                                groupList.Add("CASE" + preparedSort + "END");
                                                orderList.Add("'sortorder_" + domainArea.ColumnName + "'");

                                                //caputre the column information
                                                Columns sortedColumn = new Columns("sortorder_" + domainArea.ColumnName,
                                                    columnIndex.ToString(), "System.Int32", null);
                                                columnIndex++;

                                                columnList.Add(domainArea.ColumnName);

                                                //add the column information to the column index
                                                cellvalues.ColumnList.Add(sortedColumn);

                                            }

                                            if (orderMe)
                                            {
                                                //add the domain column to the order list 
                                                orderList.Add("'" + domainArea.ColumnName + "'");
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        //if domain is GAUL and no valid regions are provided it is a bad request
                                        return BadRequest();
                                    }
                                }
                            }
                            #endregion
                            //has pivot domain request & no geometry
                            #region Collect pivot domain select & where statements elements
                            if (pivotDomain.Id > 0 && string.IsNullOrEmpty(wktGeometry))
                            {

                                #region Prepare the inner statement logic
                                DomainArea domainArea = pivotDomain.DomainAreas[0];
                                columnList.Add(domainArea.ColumnName);
                                if (domainArea.ClassificationType.ToUpper() == "CONTINUOUS")
                                {
                                    foreach (Classification classification in domainArea.Classifications)
                                    {
                                        string statement = domainArea.ColumnName + " >= " + classification.Min +
                                            " AND " + domainArea.ColumnName + " <= " + classification.Max;

                                        //preparedSelect += " WHEN " + statement + " THEN '" + classification.Name + "'";
                                        List<string> selectStatements = new List<string>();
                                        selectStatements.Add(statement);
                                        selectStatements.Add(classification.Name);
                                        selectMatrix.Add(selectStatements);

                                        //preparedWhere.Add(statement);
                                    }
                                    processSelectMatrix(ref selectMatrix);
                                }
                                else if (domainArea.ClassificationType.ToUpper() == "CLASS")
                                {
                                    foreach (Classification classification in domainArea.Classifications)
                                    {
                                        int num = 0;
                                        bool isValueNumeric = int.TryParse(classification.Value, out num);

                                        string statement = isValueNumeric ? domainArea.ColumnName + " = " + classification.Value :
                                            domainArea.ColumnName + " = '" + fixSingleQuote(classification.Value) + "'";

                                        //preparedSelect += " WHEN " + statement + " THEN '" + classification.Name + "'";                              

                                        List<string> selectStatements = new List<string>();
                                        selectStatements.Add(statement);
                                        selectStatements.Add(classification.Name);
                                        selectMatrix.Add(selectStatements);

                                        //preparedWhere.Add(statement);
                                    }
                                    processSelectMatrix(ref selectMatrix);
                                }
                                #endregion

                            }
                            #endregion
                            //has country restriction request & no geometry
                            #region Process countries and prepare the where statement
                            if (countries.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                            {
                                string preparedSelect = string.Empty;

                                List<string> preparedWhere = new List<string>();
                                foreach (country country in countries)
                                {
                                    preparedWhere.Add("ISO3 = '" + country.id + "'");
                                    if (!usingISO3 && groupCountry)
                                    {
                                        preparedSelect += " WHEN ISO3 = '" + country.id + "' THEN '" + country.name + "'";
                                    }
                                }

                                whereList.Add("(" + String.Join(" OR ", preparedWhere.ToArray()) + ")");
                                columnList.Add("ISO3");

                                //if not using the ISO3 (Countries) as a domain and the flag to group by country is true
                                //then group by country
                                if (!usingISO3 && groupCountry)
                                {
                                    //capture column information   
                                    column = new Columns();
                                    column.ColumnName = "ISO3";
                                    column.ColumnIndex = columnIndex.ToString();
                                    column.ColumnDataType = "System.String";
                                    columnIndex++; //increment the column index                       
                                    cellvalues.ColumnList.Add(column);

                                    columnList.Add("ISO3");

                                    selectList.Add("CASE" + preparedSelect + " END AS 'Country'");
                                    groupList.Add("CASE" + preparedSelect + "END");
                                }
                            }
                            #endregion
                            //has indicator request
                            #region Process each indicator and prepare the Select statement
                            foreach (Indicator indicator in indicators)
                            {
                                column = new Columns();
                                List<string> compliedWhere = new List<string>();
                                switch (indicator.AggType.ToUpper())
                                {
                                    //note: SQL SERVER 2012 does not have a MEDIAN calculation so we are using AVG.
                                    //High-level of effor to create a custome median aggregate function
                                    #region AVG Aggregation Type
                                    case "AVG":
                                    case "MEDIAN":
                                        if (selectMatrix.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                                        {
                                            #region Process the pivot columns through select matrix
                                            foreach (List<string> statement in selectMatrix)
                                            {
                                                selectList.Add("ROUND(AVG(CASE WHEN " + statement[0] + " THEN " +
                                                    indicator.ColumnName + " ELSE 0 END), 2, 1) AS '" + statement[1] + "'");

                                                compliedWhere.Add(statement[0]);

                                                //capture column information   
                                                column = new Columns();
                                                column.ColumnName = statement[1];
                                                column.ColumnIndex = columnIndex.ToString();
                                                column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                                columnIndex++; //increment the column index                       
                                                cellvalues.ColumnList.Add(column);

                                                columnList.Add(indicator.ColumnName);
                                            }
                                            #endregion

                                            whereList.Add("(" + String.Join(" OR ", compliedWhere.ToArray()) + ")");
                                        }
                                        else
                                        {
                                            selectList.Add("ROUND(AVG(" + indicator.ColumnName + "), 2, 1) AS '" +
                                                indicator.ColumnName + "'");
                                            //capture column information  
                                            column = new Columns();
                                            column.ColumnName = indicator.ColumnName;
                                            column.ColumnIndex = columnIndex.ToString();
                                            column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                            columnIndex++; //increment the column index                       
                                            cellvalues.ColumnList.Add(column);

                                            columnList.Add(indicator.ColumnName);
                                        }
                                        break;
                                    #endregion
                                    #region SUM Aggregation Type
                                    case "SUM":
                                        if (selectMatrix.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                                        {
                                            #region Process the pivot columns through select matrix
                                            foreach (List<string> statement in selectMatrix)
                                            {
                                                selectList.Add("ROUND(SUM(CASE WHEN " + statement[0] + " THEN " +
                                                    indicator.ColumnName + " ELSE 0 END), 2, 1) AS '" + statement[1] + "'");

                                                compliedWhere.Add(statement[0]);

                                                //capture column information 
                                                column = new Columns();
                                                column.ColumnName = statement[1];
                                                column.ColumnIndex = columnIndex.ToString();
                                                column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                                columnIndex++; //increment the column index                       
                                                cellvalues.ColumnList.Add(column);

                                                columnList.Add(indicator.ColumnName);
                                            }
                                            #endregion

                                            whereList.Add("(" + String.Join(" OR ", compliedWhere.ToArray()) + ")");
                                        }
                                        else
                                        {
                                            selectList.Add("ROUND(SUM(" + indicator.ColumnName + "), 2, 1) AS '" +
                                                indicator.ColumnName + "'");
                                            //capture column information 
                                            column = new Columns();
                                            column.ColumnName = indicator.ColumnName;
                                            column.ColumnIndex = columnIndex.ToString();
                                            column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                            columnIndex++; //increment the column index                       
                                            cellvalues.ColumnList.Add(column);

                                            columnList.Add(indicator.ColumnName);
                                        }
                                        break;
                                    #endregion
                                    #region MEDIAN Aggregation Type
                                    //case "MEDIAN":
                                    //    if (selectMatrix.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                                    //    {
                                    //        #region Process the pivot columns through select matrix
                                    //        foreach (List<string> statement in selectMatrix)
                                    //        {
                                    //            selectList.Add("ROUND(MEDIAN(CASE WHEN " + statement[0] + " THEN " +
                                    //                indicator.ColumnName + " ELSE 0 END), 2, 1) AS '" + statement[1] + "'");

                                    //            compliedWhere.Add(statement[0]);

                                    //            //capture column information 
                                    //            column = new Columns();
                                    //            column.ColumnName = statement[1];
                                    //            column.ColumnIndex = columnIndex.ToString();
                                    //            column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                    //            columnIndex++; //increment the column index                       
                                    //            cellvalues.ColumnList.Add(column);

                                    //            columnList.Add(indicator.ColumnName);
                                    //        }
                                    //        #endregion

                                    //        whereList.Add("(" + String.Join(" OR ", compliedWhere.ToArray()) + ")");
                                    //    }
                                    //    else
                                    //    {
                                    //        selectList.Add("ROUND(MEDIAN(" + indicator.ColumnName + "), 2, 1) AS '" +
                                    //            indicator.ColumnName + "'");
                                    //        //capture column information 
                                    //        column = new Columns();
                                    //        column.ColumnName = indicator.ColumnName;
                                    //        column.ColumnIndex = columnIndex.ToString();
                                    //        column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                    //        columnIndex++; //increment the column index                       
                                    //        cellvalues.ColumnList.Add(column);

                                    //        columnList.Add(indicator.ColumnName);
                                    //    }
                                    //    break;
                                    #endregion
                                    #region WGHTD Aggregation Type
                                    case "WGHTD":
                                        string denominator = GetDenominator(indicator.AggFormula);
                                        string numerator = GetNumerator(indicator.AggFormula);
                                        List<string> fields = GetFields(indicator.AggFormula);
                                        string outerCalculation = indicator.AggFormula.Split(')').Last();
                                        columnList.AddRange(fields);

                                        if (selectMatrix.Count > 0 && string.IsNullOrEmpty(wktGeometry))
                                        {
                                            #region Process the pivot columns through select matrix
                                            foreach (List<string> statement in selectMatrix)
                                            {

                                                if (denominator != null)
                                                {
                                                    if (outerCalculation == null)
                                                    {
                                                        selectList.Add("CASE WHEN " + CreateAggregateCase(denominator, statement[0])
                                                            + " > 0 THEN ROUND((" + CreateAggregateCase(numerator, statement[0]) + " / "
                                                            + CreateAggregateCase(denominator, statement[0]) + "), 2, 1) ELSE 0 END AS '" +
                                                            statement[1] + "'");
                                                    }
                                                    else
                                                    {
                                                        selectList.Add("CASE WHEN " + CreateAggregateCase(denominator, statement[0])
                                                        + " > 0 THEN ROUND((" + CreateAggregateCase(numerator, statement[0]) + " / "
                                                        + CreateAggregateCase(denominator, statement[0]) + ")" + outerCalculation + ", 2, 1) ELSE 0 END AS '" +
                                                        statement[1] + "'");

                                                    }
                                                }
                                                else
                                                    selectList.Add("ROUND((" + CreateAggregateCase(indicator.AggFormula, statement[0]) + "), 2, 1) AS '" +
                                                        statement[1]);

                                                compliedWhere.Add(statement[0]);

                                                //capture column information 
                                                column = new Columns();
                                                column.ColumnName = statement[1];
                                                column.ColumnIndex = columnIndex.ToString();
                                                column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                                columnIndex++; //increment the column index                       
                                                cellvalues.ColumnList.Add(column);

                                                columnList.Add(indicator.ColumnName);
                                            }
                                            #endregion

                                            whereList.Add("(" + String.Join(" OR ", compliedWhere.ToArray()) + ")");
                                        }
                                        else
                                        {
                                            if (denominator != null)
                                                selectList.Add("CASE WHEN " + denominator + " > 0 THEN ROUND((" +
                                                    indicator.AggFormula + "), 2, 1) ELSE 0 END AS '" +
                                                    indicator.ColumnName + "'");
                                            else
                                                selectList.Add("ROUND((" + indicator.AggFormula + "), 2, 1) AS '" +
                                                    indicator.ColumnName);

                                            //capture column information 
                                            column.ColumnName = indicator.ColumnName;
                                            column.ColumnIndex = columnIndex.ToString();
                                            column.ColumnDataType = GetColumnDataType(indicator.ColumnName);
                                            columnIndex++; //increment the column index                       
                                            cellvalues.ColumnList.Add(column);

                                            columnList.Add(indicator.ColumnName);
                                        }
                                        break;
                                    #endregion
                                    default:
                                        //unknown aggregate type
                                        break;
                                }
                            }
                            #endregion
                            //has geometry request
                            #region Process Geometry
                            if (!string.IsNullOrEmpty(wktGeometry))
                            {
                                string preparedwhere = string.Empty;
                                switch (wktGeometry.Substring(0, wktGeometry.IndexOf('(')).ToUpper())
                                {
                                    //requesting a restriction of data to the extent of provided polygon or linestring
                                    case "POLYGON":
                                    case "LINESTRING":
                                        requiresGeometry = true;

                                        #region Prepare where statement
                                        preparedwhere += "( 1=1 ) AND Shape.STIntersects(geometry::STGeomFromText('";
                                        preparedwhere += wktGeometry;
                                        preparedwhere += "', 4326).MakeValid()) = 1";
                                        #endregion

                                        break;
                                    //1. requesting a restriction of data to a single cell value under provided point
                                    //2. requesting a market shed analysis based on provided point
                                    case "POINT":
                                        //requesting a market shed analysis based on provided point
                                        if (doMarketAnalysis)
                                        {

                                            //call the ArcGIS GP Service to do the market shed analysis
                                            gpOutput gpOutput = callArcGISGPService(wktGeometry);

                                            foreach (string wktPolygon in gpOutput.wktPolygons)
                                            {
                                                select = "SELECT null as 'Travel Time', "; //select string for building the sql statement

                                                #region Create From Statement
                                                string selectCellValues = "SELECT CELL5M.CELL5M,Shape";
                                                string joinsCellValues = " FROM CELL5M ";

                                                columnList = columnList.ConvertAll(d => d.ToLower());
                                                foreach (string columnName in columnList.Distinct())
                                                {
                                                    if (columnName != "cell5m")
                                                    {
                                                        selectCellValues += "," + columnName;
                                                        joinsCellValues += " LEFT OUTER JOIN " + columnName + " ON " + columnName + ".CELL5M = CELL5M.CELL5M";
                                                    }
                                                }
                                                from = " FROM ( " + selectCellValues + @joinsCellValues + ") AS CELL_VALUES";
                                                #endregion

                                                where = " WHERE "; //where string for building the sql statement
                                                groupby = " GROUP BY "; //groupby string for building the sql statement
                                                orderby = " ORDER BY "; //orderby string for building the sql statement
                                                sqlstatement = string.Empty;
                                                preparedwhere = string.Empty;
                                                whereList = new List<string>();

                                                #region Prepare where statement
                                                preparedwhere += "( 1=1 ) AND Shape.STIntersects(geometry::STGeomFromText('";
                                                preparedwhere += wktPolygon;
                                                preparedwhere += "', 4326).MakeValid()) = 1";
                                                whereList.Add(preparedwhere);
                                                #endregion

                                                #region Prepare sql statment

                                                //the select list has to at least have one value
                                                if (selectList.Count > 0)
                                                {
                                                    select += String.Join(",", selectList.ToArray());
                                                    sqlstatement = select + from;

                                                    if (whereList.Count > 0)
                                                    {
                                                        where += String.Join(" AND ", whereList.ToArray());
                                                        sqlstatement += where;
                                                    }
                                                    if (groupList.Count > 0)
                                                    {
                                                        groupby += String.Join(",", groupList.ToArray());
                                                        sqlstatement += groupby;
                                                    }
                                                    if (orderList.Count > 0)
                                                    {
                                                        orderby += String.Join(",", orderList.ToArray());
                                                        sqlstatement += orderby;
                                                    }
                                                }

                                                #endregion

                                                #region Execute the SQL Statement
                                                if (!string.IsNullOrEmpty(sqlstatement))
                                                {
                                                    SQLConnection.Open(); //Open the db conection

                                                    SqlCommand cmd = new SqlCommand(sqlstatement, SQLConnection);

                                                    SqlDataReader dataReader = cmd.ExecuteReader();


                                                    //process data
                                                    while (dataReader.Read())
                                                    {
                                                        int initilizeCount = dataReader.FieldCount;
                                                        initilizeCount++;//add a field for geometry
                                                        object[] values = new object[initilizeCount];
                                                        dataReader.GetValues(values);
                                                        cellvalues.ValueList.Add(values);
                                                    }

                                                    SQLConnection.Close(); //close the db connection
                                                }
                                                #endregion
                                            }

                                            //capture column information 
                                            column = new Columns();
                                            column.ColumnName = "Geometry";
                                            column.ColumnDesc = gpOutput.zipURL;
                                            column.ColumnIndex = columnIndex.ToString();
                                            column.ColumnDataType = "System.String";
                                            columnIndex++; //increment the column index                       
                                            cellvalues.ColumnList.Add(column);



                                            int rowCount = 0;
                                            //loop through valuelist 
                                            foreach (object[] rows in cellvalues.ValueList)
                                            {
                                                rowCount++;
                                                //row "1305.1", null, null

                                                //assign text
                                                rows[0] = rowCount * 2 + " hrs";

                                                //assign geometry
                                                rows[rows.Length - 1] = gpOutput.wktPolygons[rowCount - 1];
                                            }



                                        }
                                        //requesting a restriction of data to a single cell value under provided point
                                        else
                                        {

                                            #region Prepare the select statement
                                            //need to clear the current select statements and columns because we 
                                            //don't use aggregate formulas for indicators if there is geometry
                                            selectList = new List<string>();
                                            cellvalues.ColumnList = new List<Columns>();
                                            columnIndex = 0;
                                            foreach (Indicator indicator in indicators)
                                            {
                                                selectList.Add(indicator.ColumnName);

                                                //capture column information
                                                column = new Columns(indicator.ColumnName, columnIndex.ToString(), GetColumnDataType(indicator.ColumnName));
                                                columnIndex++; //increment the column index                       
                                                cellvalues.ColumnList.Add(column);
                                            }

                                            selectList.Add("CELL5M"); //add the primary key if its a point
                                            //capture column information
                                            column = new Columns("CELL5M", columnIndex.ToString(), "System.Int32");
                                            columnIndex++; //increment the column index                       
                                            cellvalues.ColumnList.Add(column);
                                            #endregion

                                            #region Prepare where statement
                                            preparedwhere += "( 1=1 ) AND Shape.STIntersects(geometry::STGeomFromText('";
                                            preparedwhere += wktGeometry;
                                            preparedwhere += "', 4326).MakeValid()) = 1";
                                            requiresGeometry = true;
                                            #endregion
                                        }
                                        break;
                                    default: //A valid geometry was passed, however we are not supporting it as this time
                                        return BadRequest();
                                }
                                whereList.Add(preparedwhere);
                            }
                            #endregion

                        }

                    }
                    else //if there are no indicators the only valid request is wktGeometry = POINT
                    {
                        #region Process Point Request
                        if (string.IsNullOrEmpty(wktGeometry))
                            return BadRequest();
                        else
                        {
                            string preparedwhere = string.Empty;
                            if (wktGeometry.Substring(0, wktGeometry.IndexOf('(')).ToUpper() == "POINT")
                            {
                                #region Use domains if provided
                                if (domains.Count > 0)
                                {
                                    foreach (Domains domain in domains)
                                    {

                                        foreach (DomainArea domainArea in domain.DomainAreas)
                                        {
                                            string preparedSelect = string.Empty;


                                            //This domain is too large to add all the classifications to the statment
                                            //so we handle it differently
                                            if (domain.Id == 37)
                                            {
                                                //Domain column statements
                                                selectList.Add(domainArea.ColumnName + " AS '" + domainArea.ColumnName + "'");

                                                //capture column information 
                                                column = new Columns(domainArea.ColumnName, columnIndex.ToString());
                                                column.ColumnDataType = GetColumnDataType(domainArea.ColumnName);
                                                columnIndex++; //increment the column index                       

                                                //add the column information to the return type
                                                cellvalues.ColumnList.Add(column);
                                            }
                                            else
                                            {
                                                #region Prepare the inner statement logic
                                                if (domainArea.ClassificationType.ToUpper() == "CONTINUOUS")
                                                {
                                                    foreach (Classification classification in domainArea.Classifications)
                                                    {
                                                        string statement = domainArea.ColumnName + " >= " + classification.Min +
                                                            " AND " + domainArea.ColumnName + " <= " + classification.Max;

                                                        preparedSelect += " WHEN " + statement + " THEN '" + classification.Name + "'";
                                                    }
                                                }
                                                else if (domainArea.ClassificationType.ToUpper() == "CLASS")
                                                {
                                                    foreach (Classification classification in domainArea.Classifications)
                                                    {
                                                        int num = 0;
                                                        bool isValueNumeric = int.TryParse(classification.Value, out num);
                                                        string statement = string.Empty;

                                                        statement = isValueNumeric ? domainArea.ColumnName + " = " + classification.Value :
                                                            domainArea.ColumnName + " = '" + fixSingleQuote(classification.Value) + "'";

                                                        preparedSelect += " WHEN " + statement + " THEN '" + fixSingleQuote(classification.Name) + "'";

                                                    }
                                                }
                                                #endregion
                                            }


                                            #region Assign select
                                            if (!string.IsNullOrEmpty(preparedSelect))
                                            {
                                                //Domain column statements
                                                selectList.Add("CASE" + preparedSelect + " END AS '" + domainArea.ColumnName + "'");

                                                //capture column information 
                                                column = new Columns(domainArea.ColumnName, columnIndex.ToString());
                                                column.ColumnDataType = GetColumnDataType(domainArea.ColumnName);
                                                columnIndex++; //increment the column index                       

                                                //add the column information to the return type
                                                cellvalues.ColumnList.Add(column);

                                            }
                                            #endregion

                                        }
                                    }

                                    preparedwhere += " Shape.STIntersects(geometry::STGeomFromText ('" + wktGeometry + "', 4326 )) = 1";
                                    whereList.Add(preparedwhere);
                                }
                                #endregion

                                #region otherwise give all indicators
                                else
                                {
                                    #region Prepare the select statement
                                    //Get the indicator by Id from allowed indicators
                                    List<Indicator> AllIndicators = GetAllIndicators();

                                    foreach (Indicator indicator in AllIndicators)
                                    {
                                        selectList.Add(indicator.ColumnName);
                                        //capture column information
                                        column = new Columns(indicator.ColumnName, columnIndex.ToString(), GetColumnDataType(indicator.ColumnName));
                                        columnIndex++; //increment the column index                       
                                        cellvalues.ColumnList.Add(column);
                                        columnList.Add(indicator.ColumnName);
                                    }

                                    selectList.Add("CELL5M"); //add the primary key if its a point
                                    //capture column information
                                    column = new Columns("CELL5M", columnIndex.ToString(), "System.Int32");
                                    columnIndex++; //increment the column index                       
                                    cellvalues.ColumnList.Add(column);
                                    #endregion
                                }
                                #endregion

                            }
                            else //No indicators and geometry is not a point then its an error
                            {
                                return BadRequest();
                            }
                        }

                        #endregion

                    }

                    if (!doMarketAnalysis)
                    {

                        #region Prepare sql statment
                        //the select list has to at least have one value
                        if (selectList.Count > 0)
                        {
                            select += String.Join(",", selectList.ToArray());

                            #region Create From Statement
                            string selectCellValues = "SELECT CELL5M.CELL5M";
                            if (requiresGeometry)
                                selectCellValues += ",Shape";
                            string joinsCellValues = " FROM CELL5M ";

                            columnList = columnList.ConvertAll(d => d.ToLower());
                            foreach (string columnName in columnList.Distinct())
                            {
                                if (columnName != "cell5m")
                                {
                                    selectCellValues += "," + columnName;
                                    joinsCellValues += " LEFT OUTER JOIN " + columnName + " ON " + columnName + ".CELL5M = CELL5M.CELL5M";
                                }
                            }
                            from = " FROM ( " + selectCellValues + @joinsCellValues + ") AS CELL_VALUES";
                            #endregion

                            sqlstatement = select + from;

                            if (whereList.Count > 0)
                            {
                                where += String.Join(" AND ", whereList.ToArray());
                                sqlstatement += where;
                            }
                            if (groupList.Count > 0)
                            {
                                groupby += String.Join(",", groupList.ToArray());
                                sqlstatement += groupby;
                            }
                            if (orderList.Count > 0)
                            {
                                orderby += String.Join(",", orderList.ToArray());
                                sqlstatement += orderby;
                            }
                        }
                        else
                        {
                            //need to return an custome http not found message
                        }
                        #endregion

                        #region Execute the SQL Statement
                        if (!string.IsNullOrEmpty(sqlstatement))
                        {
                            try
                            {
                                SQLConnection.Open(); //Open the db conection                                               

                                SqlCommand cmd = new SqlCommand(sqlstatement, SQLConnection);

                                SqlDataReader dataReader = cmd.ExecuteReader();

                                //process data
                                while (dataReader.Read())
                                {
                                    int initilizeCount = dataReader.FieldCount;
                                    //if returning geometry add a space for the geometry column
                                    if (returnGeometry)
                                        initilizeCount++;
                                    object[] values = new object[initilizeCount];
                                    dataReader.GetValues(values);
                                    cellvalues.ValueList.Add(values);
                                }
                            }
                            catch (Exception ex)
                            {
                                return BadRequest();
                            }
                            finally
                            {
                                SQLConnection.Close(); //close the db connection
                            }
                        }
                        #endregion

                        #region Process request to return geometry
                        //must have: valid single domain, at least one indicator, return geometry flag true
                        //must not have: incoming geometry or pivotDomainId
                        if (domains.Count == 1 && indicators.Count > 0 && returnGeometry == true
                            && string.IsNullOrEmpty(wktGeometry) && pivotDomainId == 0)
                        {
                            //capture column information 
                            column = new Columns();
                            column.ColumnName = "Geometry";
                            column.ColumnIndex = columnIndex.ToString();
                            column.ColumnDataType = "System.String";
                            columnIndex++; //increment the column index                       
                            cellvalues.ColumnList.Add(column);

                            //prepare the sql statement
                            sqlstatement = string.Empty; //clean it out for use
                            sqlstatement = "SELECT name, geometry::UnionAggregate(Shape.MakeValid()).ToString() AS Geom ";
                            sqlstatement += "FROM dbo.domain_country_results ";
                            sqlstatement += "WHERE schema_id = " + domains[0].Id + " ";

                            //check if there are isos and add to statement
                            if (countries.Count > 0)
                            {
                                List<string> preparedWhere = new List<string>();
                                foreach (country country in countries)
                                {
                                    preparedWhere.Add("'" + country.id + "'");
                                }

                                sqlstatement += "AND ISO3 IN (" + String.Join(", ", preparedWhere.ToArray()) + ") ";
                            }
                            sqlstatement += "GROUP BY name";


                            CellValues geometryValues = new CellValues();
                            //append to cellvalues object
                            if (!string.IsNullOrEmpty(sqlstatement))
                            {
                                SQLConnection.Open(); //Open the db conection

                                SqlCommand cmd = new SqlCommand(sqlstatement, SQLConnection);

                                SqlDataReader dataReader = cmd.ExecuteReader();


                                //process data
                                while (dataReader.Read())
                                {
                                    object[] values = new object[dataReader.FieldCount];
                                    dataReader.GetValues(values);
                                    geometryValues.ValueList.Add(values);
                                }

                                SQLConnection.Close(); //close the db connection
                            }

                            //collect domain column indexes
                            List<int> domainColumnIndexes = new List<int>();
                            foreach (Columns c in cellvalues.ColumnList)
                            {
                                if (isDomainColumn(c, domains[0]))
                                    domainColumnIndexes.Add(Convert.ToInt32(c.ColumnIndex));
                            }

                            //loop through valuelist 
                            foreach (object[] rows in cellvalues.ValueList)
                            {
                                //row "Arid", "1", "Lo","3",234.33

                                string key = string.Empty;
                                foreach (int idx in domainColumnIndexes)
                                {
                                    key += rows[idx].ToString().ToUpper();
                                }
                                //loop through the geometry data see if we have a match
                                foreach (object[] geom in geometryValues.ValueList)
                                {
                                    if (geom[0].ToString().ToUpper() == key)
                                    {
                                        rows[rows.Length - 1] = geom[1];
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    return Ok(cellvalues);
                    
                }
                catch (Exception ex) {
                    return InternalServerError();
                }
            }
            else
            {
                return BadRequest();
            }
        }           

        private List<Indicator> ValidateIndicators(int[] indicatorIds)
        {
            List<Indicator> indicators = new List<Indicator>();//list of valid indicators

            if (indicatorIds != null && indicatorIds.Length != 0)
            {
                //get indicators
                foreach (int indicatorId in indicatorIds)
                {
                    try
                    {

                        //Get the indicator by Id from allowed indicators
                        indicator_metadata indicator_metadata = db.indicator_metadata.SingleOrDefault(x =>
                            x.ID == indicatorId
                            && x.published == true
                            && x.genRaster == true);

                        //Invalid indicator requests are ignored
                        if (indicator_metadata != null)
                        {
                            Indicator indicator = new Indicator();
                            indicator.Id = indicator_metadata.ID;
                            indicator.ColumnName = indicator_metadata.varCode;
                            indicator.MicroLabel = indicator_metadata.varLabel;
                            indicator.Unit = indicator_metadata.unit;
                            indicator.Year = indicator_metadata.year;
                            indicator.DecimalPlaces = indicator_metadata.dec;
                            indicator.ClassificationType = indicator_metadata.type;
                            indicator.AggType = indicator_metadata.aggType;
                            indicator.AggFormula = indicator_metadata.aggFun;

                            indicators.Add(indicator);
                        }
                    }
                    catch (Exception ex)
                    {
                        //more than likely exception due to invalid indicator id
                        //so we are just going to ignore it
                    }
                }
                if (indicators.Count > 0)
                    //remove all indicators that are not of type continuous
                    indicators.RemoveAll(x => x.IsDomain == true);
            }

            return indicators;

        }

        private List<Domains> ValidateDomains(int[] domainIds)
        {
            List<Domains> domains = new List<Domains>();//list of valid domains

            //get domains
            foreach (int domainId in domainIds)
            {
                try
                {
                    //get the domain from the allowed domains
                    schema schema = db.schemata.SingleOrDefault(x =>
                        x.id == domainId
                        && (x.published == true || x.published == null)
                        && (x.for_mappr == true || x.for_mappr == null));

                    //invalid domains are ignored
                    if (schema != null)
                    {
                        //flag if using country boundries as a domain
                        if (schema.id == 33)
                            usingISO3 = true;
                        //flag if using gual boundries as a domain
                        if (schema.id == 37)
                            usingGAUL = true;

                        Domains domain = new Domains();
                        domain.Id = schema.id;
                        domain.Description = schema.description;
                        domain.Name = schema.name;
                        domain.DomainAreas = schema.schema_domain.AsEnumerable()
                            .Select(x => new DomainArea
                            {
                                Id = x.id,
                                ColumnName = x.domain_variable.column_name,
                                MicroLabel = x.domain_variable.micro_label,
                                LongDescription = x.domain_variable.long_description,
                                Unit = x.domain_variable.unit,
                                Year = x.domain_variable.year,
                                DecimalPlaces = x.domain_variable.decimal_places,
                                ClassificationType = x.domain_variable.classification_type,
                                AggType = x.domain_variable.agg_type,
                                AggFormula = x.domain_variable.agg_formula,
                                Classifications = x.domain_variable.classification_type.ToUpper() == "CONTINUOUS" ?
                                x.domain_variable.continuous_classification.AsEnumerable()
                                    .Select(cc => new Classification
                                    {
                                        Name = cc.classification.name,
                                        Max = cc.max,
                                        Min = cc.min,
                                        SortOrder = cc.sortorder
                                    }).ToList() :
                                x.domain_variable.discrete_classification.AsEnumerable()
                                    .Select(dc => new Classification
                                    {
                                        Name = dc.classification.name,
                                        Value = dc.originalid,
                                        SortOrder = dc.sortorder
                                    }).ToList()
                            }).ToList();

                        domains.Add(domain);
                    }
                }
                catch (Exception ex)
                {
                    //more than likely exception due to invalid domain id
                }
            }

            return domains;
        }        

        private Domains ValidatePivotDomain(int domainId) { 
            //domains that have more than one DomainArea can not be a pivot domain
            Domains domain = new Domains(); //valid domain

                try
                {
                    //get the domain from the allowed domains
                    schema schema = db.schemata.SingleOrDefault(x =>
                        x.id == domainId
                        && (x.published == true || x.published == null)
                        && (x.for_mappr == true || x.for_mappr == null)
                        && x.schema_domain.Count() == 1); //only include schemas with one domain

                    //invalid domains are ignored
                    if (schema != null)
                    {
                        //flag if using country boundries as a domain
                        if (schema.id == 33)
                            usingISO3 = true;
                        //flag if using gual boundries as a domain
                        if (schema.id == 37)
                            usingGAUL = true;

                        domain.Id = schema.id;
                        domain.Description = schema.description;
                        domain.Name = schema.name;
                        domain.DomainAreas = schema.schema_domain.AsEnumerable()
                            .Select(x => new DomainArea
                            {
                                Id = x.id,
                                ColumnName = x.domain_variable.column_name,
                                MicroLabel = x.domain_variable.micro_label,
                                LongDescription = x.domain_variable.long_description,
                                Unit = x.domain_variable.unit,
                                Year = x.domain_variable.year,
                                DecimalPlaces = x.domain_variable.decimal_places,
                                ClassificationType = x.domain_variable.classification_type,
                                AggType = x.domain_variable.agg_type,
                                AggFormula = x.domain_variable.agg_formula,
                                Classifications = x.domain_variable.classification_type.ToUpper() == "CONTINUOUS" ?
                                x.domain_variable.continuous_classification.AsEnumerable()
                                    .Select(cc => new Classification
                                    {
                                        Name = cc.classification.name,
                                        Max = cc.max,
                                        Min = cc.min,
                                        SortOrder = cc.sortorder
                                    }).ToList() :
                                x.domain_variable.discrete_classification.AsEnumerable()
                                    .Select(dc => new Classification
                                    {
                                        Name = dc.classification.name,
                                        Value = dc.originalid,
                                        SortOrder = dc.sortorder
                                    }).ToList()
                            }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    //more than likely exception due to invalid domain id
                }           

            return domain;
        }

        private List<country> ValidateCountries(string[] countryIds)
        {
            List<country> countries = new List<country>();//list of valid countries

            foreach (string countryId in countryIds)
            {
                try
                {
                    country country = db.countries.SingleOrDefault(x =>
                        x.id == countryId);

                    if (country != null)
                    {
                        countries.Add(country);
                    }
                }
                catch (Exception ex)
                {
                    //more than likely exception due to invalid country
                }
            }

            return countries;
        }

        private List<CountryRegion> ValidateRegions(string[] countryNames)
        {
            int regionCount = 0; //number of valid regions
            int regionLimit = 3; //number of allowed valid regions
            bool regionLimitReached = false; //has the allowed valid region limit been reached?
            List<CountryRegion> countryRegions = new List<CountryRegion>();//list of valid regions/districts

            foreach (string countryName in countryNames)
            {
                //skip logic if the number of valid regions has been met
                //we only accept a limited number of regions
                //if (!regionLimitReached)
                //{
                    try
                    {
                        //query cell values for region/district
                        var results = (from r in db.vGAUL_2008
                                       where r.ISO3 == countryName
                                       select new { r.ISO3, r.GAUL_2008_1 }).Distinct().ToList();

                        if (results != null && results.Count > 0)
                        {
                            CountryRegion countryRegion = new CountryRegion();
                            List<Region> regions = new List<Region>();

                            //collect the country name
                            countryRegion.Name = results.First().ISO3;
                            //collect the district names
                            foreach (var result in results)
                            {
                                Region region = new Region();
                                region.Name = result.GAUL_2008_1;
                                regions.Add(region);
                            }

                            if (regions.Count > 0)
                            {
                                countryRegion.Regions = regions;
                            }

                            countryRegions.Add(countryRegion);
                        }
                    }
                    catch (Exception ex)
                    {
                        //more than likely exception due to invalid region name
                    }
               // }
            }

            return countryRegions;
        }

        private List<Indicator> GetAllIndicators()
        {

            var result = (from i in db.indicator_metadata
                          where i.published == true || i.published == null
                          select new
                          {
                              i.ID,
                              i.varCode,
                              i.varLabel,
                              i.unit,
                              i.year,
                              i.dec,
                              i.type,
                              i.aggType,
                              i.aggFun
                          }).ToList();

            List<Indicator> indicators = result.AsEnumerable()
                .Select(o => new Indicator
                {
                    Id = o.ID,
                    ColumnName = o.varCode,
                    MicroLabel = o.varLabel,
                    Unit = o.unit,
                    Year = o.year,
                    DecimalPlaces = o.dec,
                    ClassificationType = o.type,
                    AggType = o.aggType,
                    AggFormula = o.aggFun
                }).ToList();

            return indicators;
        }

        private string GetDenominator(string formula)
        {
            string trimmed_formula;
            if (formula != null)
            {
                try
                {
                    //formula begins with ( 
                    if (formula.IndexOf('(') == 0)
                    {
                        trimmed_formula = formula.Substring(formula.IndexOf('(') + 1, formula.LastIndexOf(')') - 1);
                    }
                    else {
                        trimmed_formula = formula;
                    }
                    string[] formulaSections = trimmed_formula.Split('/');
                    
                    return formulaSections[1];
                }
                catch (Exception ex)
                {
                    //there was a problem getting the denominator from the formula
                    return null;
                }
            }
            return null;
        }

        private string GetNumerator(string formula)
        {
            string trimmed_formula;
            if (formula != null)
            {
                try
                {
                    //formula begins with ( 
                    if (formula.IndexOf('(') == 0)
                    {
                        trimmed_formula = formula.Substring(formula.IndexOf('(') + 1, formula.LastIndexOf(')') - 1);
                    }
                    else
                    {
                        trimmed_formula = formula;
                    }
                    string[] formulaSections = trimmed_formula.Split('/');

                    return formulaSections[0];
                }
                catch (Exception ex)
                {
                    //there was a problem getting the denominator from the formula
                    return null;
                }
            }
            return null;
        }

        private string CreateAggregateCase(string formula, string statement)
        { 
            if(!string.IsNullOrEmpty(formula) && !string.IsNullOrEmpty(statement))
            {
                //parse formula
                string operation = formula.Split('(').First();
                string fields = formula.Substring(formula.IndexOf('(') + 1, formula.IndexOf(')') - formula.IndexOf('(') - 1);
                string outerCalculation = formula.Split(')').Last();

                string aggCase = " " + operation + "(CASE WHEN " + statement + " THEN " + fields + " ELSE 0 END) ";
                if (!string.IsNullOrEmpty(outerCalculation))
                    aggCase += outerCalculation;

                return aggCase;
            }
            else
                return null;
        }

        private List<string> GetFields(string formula) {
            List<string> goodWords = new List<string>();
            char[] badChars = { ' ', '/', '*', '(', ')' };
            string[] badWords = { "SUM" };

            string[] words = formula.Split(badChars);
            double num;
            foreach (string s in words)
            {
                if (!double.TryParse(s, out num) && !badWords.Contains(s) && s != "")
                {
                    if(!goodWords.Contains(s))
                        goodWords.Add(s);
                }
            }
            return goodWords;
        }

        private string GetColumnDataType(string columnName)
        {
            try
            {
                indicator_metadata indicator = db.indicator_metadata.SingleOrDefault(x =>
                       x.varCode == columnName);
                if (indicator != null)
                    return indicator.dataType;
                else
                    return null;
            }
            catch (Exception ex)
            {
                return "null";
            }
        }

        private void processSelectMatrix(ref List<List<string>> selectMatrix)
        {
            List<List<string>> groupedSelectMatrix = new List<List<string>>();


            foreach (List<string> select in selectMatrix)
            {
                bool hasMatch = false;

                foreach (List<string> groupedSelect in groupedSelectMatrix) {
                    if (select[1] == groupedSelect[1])
                    {
                        groupedSelect[0] += " OR " + select[0];
                        hasMatch = true;
                    }
                }
                if (!hasMatch)
                {
                    List<string> newGroup = new List<string>();
                    newGroup.Add(select[0]);
                    newGroup.Add(select[1]);
                    groupedSelectMatrix.Add(newGroup);
                }
            }

            selectMatrix = new List<List<string>>();
            selectMatrix = groupedSelectMatrix;
        }

        private bool isDomainColumn(Columns c, Domains d) {

            foreach (DomainArea area in d.DomainAreas)
            {
                if (area.ColumnName.ToUpper().Trim() == c.ColumnName.ToUpper().Trim())
                    return true;
            }

            return false;
        }

        //Returns a string equal to input string but with all single quotes
        //fixed up for database insertion.
        private string fixSingleQuote(string input)
        {            
            StringBuilder output = new StringBuilder(input);
            if (input != null)
            {
                int length = input.Length;
                int j = 0; //Increments index for StringBuilder
                for (int i = 0; i < length; i++)
                {
                    //Check if single quote exists
                    if (input[i].Equals('\''))
                    {
                        //If exists insert single quote in output
                        output.Insert(j, '\'');
                        //Increment output index for inserted char
                        j++;
                    }
                    j++;
                }
            }
            return output.ToString();
        }
        /// <summary>
        /// Call to the ArcGIS Geoprocessing Service to calculate the market shed polygons
        /// for 2,4,6,8 hour tavel time zones for a given point.
        /// </summary>
        /// <param name="wktPoint">point of origin for analysis</param>
        /// <returns>List of string containing wktPolygon descriptions</returns>
        private gpOutput callArcGISGPService(string wktPoint)
        {

            gpOutput gpOutput = new gpOutput();            
            List<string> wktPolygons = new List<string>(); //return wkt polygons
            gpOutput.wktPolygons = wktPolygons;
            DbGeometry point = DbGeometry.PointFromText(wktPoint, 102100); //passed point

            #region call the geoprocessing task for market shed
            string query = "/submitJob?f=json&Input_Location={'features':[{'geometry':{'x':"
                + point.XCoordinate + ",'y':" + point.YCoordinate + "}}]}";

            string call = gpURL + query;

            HttpWebRequest request = WebRequest.Create(call) as HttpWebRequest;
            gpJob job = new gpJob();
            #endregion

            #region get job information
            using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());

                JavaScriptSerializer js = new JavaScriptSerializer();

                job = (gpJob)js.Deserialize(reader.ReadToEnd(), typeof(gpJob));
            }
            #endregion

            if (job.jobStatus == "esriJobSubmitted")
            {
                gpJob result = new gpJob();

                #region wait for job to finish
                do
                {
                    string checkJobStatus = gpURL + "/jobs/" + job.jobId + "?f=json";

                    request = WebRequest.Create(checkJobStatus) as HttpWebRequest;

                    using (HttpWebResponse status = request.GetResponse() as HttpWebResponse)
                    {
                        StreamReader reader = new StreamReader(status.GetResponseStream());

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        result = (gpJob)js.Deserialize(reader.ReadToEnd(), typeof(gpJob));
                    }
                } while (result.jobStatus == "esriJobExecuting" || result.jobStatus == "esriJobSubmitted");
                #endregion

                string getOutput = gpURL + "/jobs/" + job.jobId + "/results/OutputShape?f=json";
                string getZip = gpURL + "/jobs/" + job.jobId + "/results/zipName?f=json";                

                #region process json output of geoprocessing task
                request = WebRequest.Create(getOutput) as HttpWebRequest;
                using (HttpWebResponse status = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(status.GetResponseStream());
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var gpoutput = js.Deserialize<dynamic>(reader.ReadToEnd());

                    List<List<string>> jsonRing = new List<List<string>>();
                    List<string> jsonPoints = new List<string>();
                    List<string> ringCollection = new List<string>();                                        

                    #region iterate over output to find geometry
                    foreach (var output in gpoutput) {
                        if (output.Key == "value")
                        {
                            foreach (var value in output.Value)
                            {
                                //find features
                                if (value.Key == "features")
                                {
                                    foreach (var feature in value.Value)
                                    {
                                        foreach (var f in feature)
                                        {
                                            //find geometry
                                            if (f.Key == "geometry")
                                            {
                                                

                                                #region transform json geometry to wkt geometry
                                                foreach (var geometry in f.Value)
                                                {
                                                    jsonPoints = new List<string>();
                                                    jsonRing = new List<List<string>>();
                                                    foreach (var ring in geometry.Value)
                                                    {
                                                        jsonPoints = new List<string>();
                                                        decimal[] firstPoint = new decimal[2];
                                                        int numberOfPoints = ring.Length;
                                                        int pointCounter = 0;
                                                        foreach (var pointset in ring)
                                                        {
                                                            jsonPoints.Add(pointset[0] + " " + pointset[1]);

                                                            pointCounter++;
                                                            //first point
                                                            if (pointCounter == 1){
                                                                firstPoint[0] = (decimal)pointset[0];
                                                                firstPoint[1] = (decimal)pointset[1];
                                                            }
                                                            //last point
                                                            if (pointCounter == numberOfPoints) {
                                                                decimal x = pointset[0];
                                                                decimal y = pointset[1];
                                                                if (!firstPoint[0].Equals(x) || !firstPoint[1].Equals(y))
                                                                {
                                                                    jsonPoints.Add(firstPoint[0] + " " + firstPoint[1]);
                                                                }
                                                            }                                                            
                                                        }
                                                        jsonRing.Add(jsonPoints);
                                                    }

                                                    ringCollection = new List<string>();
                                                    foreach (List<string> ptcoll in jsonRing)
                                                    {
                                                        ringCollection.Add(
                                                        "(" + String.Join(",", ptcoll.ToArray()) + ")"
                                                        );
                                                    }
                                                    wktPolygons.Add(
                                                        "POLYGON(" + String.Join(",", ringCollection.ToArray()) + ")"
                                                        );
                                                }
                                                #endregion
                                            }
                                        }
                                    }//end feature loop
                                }
                            }//end value loop
                        }
                    }//end output loop
                    #endregion
                }
                #endregion

                #region capture zip url of data from geoprocessing task
                request = WebRequest.Create(getZip) as HttpWebRequest;
                using (HttpWebResponse status = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(status.GetResponseStream());
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var output = js.Deserialize<dynamic>(reader.ReadToEnd());
                    foreach (var data in output) {
                        if (data.Key == "value")
                        {
                            gpOutput.zipURL = data.Value;
                        }
                    }
                }
                #endregion
            }

            return gpOutput;
        }

        private class gpJob {
            public string jobId { get; set; }
            public string jobStatus { get; set; }            

        }

        private class gpOutput{
            public string zipURL { get; set; }
            public List<string> wktPolygons { get; set; }
        }

    
    }
}