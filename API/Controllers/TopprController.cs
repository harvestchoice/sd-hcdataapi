using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HarvestChoiceApi.Models;
using HarvestChoiceApi.Documentation.Models;

namespace HarvestChoiceApi.Controllers
{
    public class TopprController : ApiController
    {
        //EF Connection
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();
        //Database connection
        private SqlConnection SQLConnection =
                new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["HC_WEB_DB_2"].ConnectionString);

        [ApiReturnType(typeof(CellValues))]
        [HttpGet]
        public List<CellValues> GetToppr([FromUri] string[] countryName = null, [FromUri] string GAULRegion = null, [FromUri]string wktGeometry = null)
        {
            //must include one parameter
            if (countryName != null || GAULRegion != null || wktGeometry != null)
            {
                #region Declarations
                List<CellValues> cellValuesList = new List<CellValues>();//returned object
                List<string> validCountryNames = new List<string>();
                Columns column = new Columns();
                List<string> topprTables = new List<string>();
                    topprTables.Add("toppr_area");
                    topprTables.Add("toppr_prod");
                    topprTables.Add("toppr_value");
                int GAULYear = 0;              
                string where = " WHERE "; //where string for building the sql statement                
                string sqlstatement = string.Empty;
                int columnIndex = 0;
                #endregion

                #region Validate Parameters
                //Validate region 
                if(GAULRegion != null)
                    GAULYear = ValidateRegionYear(ref GAULRegion);
              
                //Validate country names
                validCountryNames = ValidateCountryNames(countryName);

                if (validCountryNames.Count < 1 && GAULYear==0 && wktGeometry==null) 
                    throw new HttpResponseException(HttpStatusCode.BadRequest);               
                #endregion

                //determine where statement
                if (validCountryNames.Count > 0)
                    where += " ISO3 IN ('" + string.Join("','", validCountryNames.ToArray()) + "')";
                else if (GAULYear > 0)
                    where += " GAUL_" + GAULYear + "_1 = '" + GAULRegion + "'";
                else if (wktGeometry != null)
                    where += " CELL5M IN (SELECT CELL5M.CELL5M FROM CELL5M WHERE ( 1=1 ) " +
                        "AND Shape.STIntersects(geometry::STGeomFromText('" + wktGeometry + "', 4326).MakeValid()) = 1)";

                //must have where defined (per request)
                if (where != null)
                {
                    // Harvested Area, Production and Value of Production
                    foreach (string topprTable in topprTables)
                    {
                        CellValues cellValues = new CellValues();

                        #region Create Common Columns
                        column = new Columns();
                        column.ColumnName = "Rank";
                        column.ColumnIndex = columnIndex.ToString();
                        column.ColumnDataType = "System.Int32";
                        columnIndex++; //increment the column index                       
                        cellValues.ColumnList.Add(column);

                        column = new Columns();
                        column.ColumnName = "Commodity";
                        column.ColumnIndex = columnIndex.ToString();
                        column.ColumnDataType = "System.String";
                        columnIndex++; //increment the column index                       
                        cellValues.ColumnList.Add(column);
                        #endregion

                        switch (topprTable)
                        {
                            case "toppr_area":
                                #region Create Table Specific Columns
                                column = new Columns();
                                column.ColumnName = "Harvested Area (ha)";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.Float";
                                columnIndex++; //increment the column index                       
                                cellValues.ColumnList.Add(column);

                                column = new Columns();
                                column.ColumnName = "Pct. of Total Area";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.Float";
                                columnIndex++; //increment the column index                       
                                cellValues.ColumnList.Add(column);
                                #endregion
                                sqlstatement = "SELECT ROW_NUMBER() OVER (ORDER BY t DESC) AS r, c, t, p FROM " +
                                "(SELECT TOP 5 (select cat3 from indicator_metadata where varCode = crop) as c," +
                                " ROUND(SUM(value),2,1) as t, ROUND(100*SUM(value)/SUM(area_crop),1,1) as p " +
                                "FROM " + topprTable + where + " GROUP BY crop ORDER BY t desc) as top_crops";
                                break;
                            case "toppr_prod":
                                #region Create Table Specific Columns
                                column = new Columns();
                                column.ColumnName = "Production (mt)";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.Float";
                                columnIndex++; //increment the column index                       
                                cellValues.ColumnList.Add(column);
                                #endregion
                                sqlstatement = "SELECT ROW_NUMBER() OVER (ORDER BY t DESC) AS r, c, t FROM " +
                                "(SELECT TOP 5 (select cat3 from indicator_metadata where varCode = crop) as c," + 
                                " ROUND(SUM(value),2,1) as t " +
                                "FROM " + topprTable + where + " GROUP BY crop ORDER BY t desc) as top_crops";
                                break;
                            case "toppr_value":
                                #region Create Table Specific Columns
                                column = new Columns();
                                column.ColumnName = "Value of Production (int$)";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.Float";
                                columnIndex++; //increment the column index                       
                                cellValues.ColumnList.Add(column);

                                column = new Columns();
                                column.ColumnName = "Pct. of Total Value";
                                column.ColumnIndex = columnIndex.ToString();
                                column.ColumnDataType = "System.Float";
                                columnIndex++; //increment the column index                       
                                cellValues.ColumnList.Add(column);
                                #endregion
                                sqlstatement = "SELECT ROW_NUMBER() OVER (ORDER BY t DESC) AS r, c, t, p FROM " +
                                "(SELECT TOP 5 (select cat3 from indicator_metadata where varCode = crop) as c," +
                                " ROUND(SUM(value),2,1) as t, ROUND(100*SUM(value)/SUM(vp_crop),1,1) as p " +
                                "FROM " + topprTable + where + " GROUP BY crop ORDER BY t desc) as top_crops";
                                break;
                        }

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
                                    object[] values = new object[initilizeCount];
                                    dataReader.GetValues(values);
                                    cellValues.ValueList.Add(values);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new HttpResponseException(HttpStatusCode.BadRequest);
                            }
                            finally
                            {
                                SQLConnection.Close(); //close the db connection
                            }
                        }
                        #endregion

                        cellValuesList.Add(cellValues);
                    }
                }
                else {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);               
                }

                return cellValuesList;
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        private int ValidateRegionYear(ref string regionName)
        {
            string name = regionName.ToLower();
            if (regionName == null)
                return 0;

            List<string> GAUL2012List = db.GAUL_2012_1.Select(x => x.GAUL_2012_11.ToLower()).Distinct().ToList();

            if (GAUL2012List.Contains(regionName.ToLower()))
            {
                var gaul = db.GAUL_2012_1.First(x => x.GAUL_2012_11.ToLower() == name);
                regionName = gaul.GAUL_2012_11;
                return 2012;
            }

            List<string> GAUL2008List = db.GAUL_2008_1.Select(x => x.GAUL_2008_11.ToLower()).Distinct().ToList();

            if (GAUL2008List.Contains(regionName.ToLower()))
            {
                var gaul = db.GAUL_2008_1.First(x => x.GAUL_2008_11.ToLower() == name);
                regionName = gaul.GAUL_2008_11;
                return 2008;
            }

            return 0;
        }

        private List<string> ValidateCountryNames(string[] countryNames)
        {
            List<string> validNames = new List<string>();

            if (countryNames == null)
                return null;

            List<string> CountryList = db.ISO3.Select(x => x.ISO31.ToLower()).Distinct().ToList();

            foreach (string countryName in countryNames) {
                if (CountryList.Contains(countryName.ToLower()))
                    validNames.Add(countryName.ToUpper());

            }
            
            return validNames;
        }
    }
}
