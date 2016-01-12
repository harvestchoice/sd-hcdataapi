using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HarvestChoiceApi.Models;
using System.Web.Http.OData.Query;
using HarvestChoiceApi.Classes;
using HarvestChoiceApi.Documentation.Models;

namespace HarvestChoiceApi.Controllers
{

    public class CategoriesController : ApiController
    {
        private HarvestChoiceApi.Models.HC_Entities db = new HarvestChoiceApi.Models.HC_Entities();

        /// <summary>
        /// Gets all indicators and groups them by category. 
        /// </summary>
        /// <remarks>All indicators are grouped by their category fields (cat1, cat2, cat3). The categories are hierarchial and labeled as follows: (cat1), sub-category (cat2) and aggregate (cat3). For more information on indicator see the Indicators method.</remarks>
        /// <example>http://dev.harvestchoice.org/HarvestChoiceApi/0.1/api/categories</example>
        /// <returns>A list of all indicators grouped by category. </returns>

        [Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
        [ApiReturnType(typeof(Category))]
        public IEnumerable<Category> GetCategories([FromUri] bool returnDoamins = true)
        {

            List<Category> CategoryList = new List<Category>(); //List of category classes
            bool hasIndicators = false;

            //Get all the cateogries
            IEnumerable<string> categories = AllCategories();

            #region cycle through each category and collect the sub categories
            foreach (string category in categories)
            {
                Category c = new Category();
                c.Name = category;
                c.Subcategories = new List<Subcategory>();

                //Get all the subcategories for the given category
                IEnumerable<string> subCategories = SubCategoriesByCategory(category);


                #region cycle through each subcategory and collect the aggregates
                foreach (string subcategory in subCategories)
                {
                    Subcategory s = new Subcategory();
                    s.Name = subcategory;
                    s.Aggregates = new List<Aggregate>();

                    //Get all the aggregate values
                    IEnumerable<string> aggregates = AggregatesBySubCategory(category, subcategory);

                    #region cycle through each aggregate and collect the indicators
                    foreach (string aggregate in aggregates)
                    {

                        Aggregate a = new Aggregate();
                        a.Name = aggregate;
                        a.Indicators = new List<Indicator>();
                        s.Aggregates.Add(a);

                        IEnumerable<indicator_metadata> indicators;

                        if (returnDoamins)
                        {
                            //Get all the indicator values
                            indicators = IndicatorsByCategories(category, subcategory, aggregate, returnDoamins);
                        }
                        else {
                            //Get all the indicator values
                            indicators = IndicatorsByCategories(category, subcategory, aggregate, returnDoamins);                        
                        }

                        hasIndicators = indicators.Any();

                        foreach (indicator_metadata indicator in indicators)
                        {
                            Indicator i = new Indicator();
                            i.Id = indicator.ID;
                            i.ColumnName = indicator.varCode;
                            i.MicroLabel = indicator.varLabel;
                            i.ShortLabel = indicator.varLabel;
                            i.FullLabel = indicator.varTitle;
                            i.Unit = indicator.unit;
                            i.Year = indicator.year;
                            i.EndYear = indicator.yearEnd;
                            i.DecimalPlaces = indicator.dec;
                            i.ClassificationType = indicator.type;
                            i.AggType = indicator.aggType;
                            i.AggFormula = indicator.aggFun;
                            i.LongDescription = indicator.varDesc;
                            i.Source = indicator.sources;
                            i.DataWorker = indicator.owner;
                            i.IsDomain = (bool)indicator.isDomain;
                            a.Indicators.Add(i);
                        }
                    }
                    if (hasIndicators)
                    {
                        c.Subcategories.Add(s);
                    }
                    #endregion
                }
                #endregion

                CategoryList.Add(c);
            }
            #endregion

            return CategoryList;            
        }

        /// <summary>
        /// Gets all the distinct cat1 values from the 
        /// indicator_metadata table. 
        /// </summary>
        /// <returns>List of categories</returns>
        private IEnumerable<string> AllCategories()
        {           
            //select all distinct cat1 values where published is 1 (true) or null
            var categories =
                (from i in db.indicator_metadata.Where(x => x.published == true && x.genRaster == true)
                    select i.cat1).Distinct();

            //if no categories were found throw not found error
            if (categories == null) throw Exceptions.NotFound("No categories were found");
                
            return categories;                
        }
        
        /// <summary>
        /// Gets all distinct sub-category (cat2) values from the 
        /// indicator_metadata table where cat1 is 
        /// <paramref name="cateory"/>.
        /// </summary>
        /// <param name="category">A category (cat1) value.</param>
        /// <returns>List of sub-categories (cat2).</returns>
        private IEnumerable<string> SubCategoriesByCategory(string category)
        {
            //select all distinct cat2 values where published is 1 (true) or null
            //and cat1 matches incoming category
            var subCategories =
                (from i in db.indicator_metadata.Where(x => x.published == true
                    && x.genRaster == true
                    && x.cat1 == category)
                 select i.cat2).Distinct();

            //If no values are returned throw not found error
            if (subCategories == null) throw Exceptions.NotFound("No subCategories (cat2) were found for category (cat1) '" + category + "'");

            return subCategories;

            ////Get all the categories
            //IEnumerable<string> categories = AllCategories();

            ////Check to make sure incoming category exists
            //if (categories.Contains(category))
            //{
                
            //}
            //else //the incoming category doesn't exist
            //    throw Exceptions.NotFound("Invalid category (cat1). '" + category + "' is not a valid value.");             
        }

        /// <summary>
        /// Gets all distinct aggregate (cat3) values from the
        /// indicator_metadata table where cat1 is <paramref name="category"/>
        /// and cat2 is <paramref name="subCategory"/>.
        /// </summary>
        /// <param name="category">A category (cat1) value.</param>
        /// <param name="subCategory">A sub-category (cat2) value.</param>
        /// <returns>List of aggregates (cat3)</returns>
        private IEnumerable<string> AggregatesBySubCategory(string category, string subCategory)
        {
            //select all distinct cat3 values where published is 1 (true) or null
            //and cat1 matches incoming category and cat2 matches incoming subcategory
            var aggregates =
                 (from i in db.indicator_metadata.Where(x => x.published == true
                     && x.genRaster == true
                     && x.cat1 == category
                     && x.cat2 == subCategory)
                  select i.cat3).Distinct();

            //return values even if there were no values found - nulls ok
            return aggregates;  

            ////Get all the categories
            //IEnumerable<string> categories = AllCategories();

            ////Check to make sure incoming category exists
            //if (categories.Contains(category))
            //{
            //    //Get the sub-categories for the category
            //    IEnumerable<string> subcategories = SubCategoriesByCategory(category);

            //    //Check to make sure the incoming subCategory is valid
            //    if (subcategories.Contains(subCategory))
            //    {
                   
            //    }
            //    else //the incoming subcategory doesn't exist
            //        throw Exceptions.NotFound("Invalid subCategory (cat2). '" + subCategory + "' is not a valid value."); 
            //}
            //else //the incoming category doesn't exist
            //    throw Exceptions.NotFound("Invalid category (cat1). '" + category + "' is not a valid value."); 
                      
        }

        /// <summary>
        /// Gets all indicators from the indicator_metadata table where 
        /// cat1 is <paramref name="category"/>
        /// and cat2 is <paramref name="subCategory"/>
        /// and cat3 is <paramref name="aggregate"/>.
        /// </summary>
        /// <param name="category">A category (cat1) value.</param>
        /// <param name="subCategory">A sub-category (cat2) value.</param>
        /// <param name="aggregate">A aggregate (cat3) value.</param>
        /// <returns>A list of <typeparamref name="indicator_metadata"/> indicators.</returns>
        private IEnumerable<indicator_metadata> IndicatorsByCategories(string category, string subCategory, string aggregate, bool returnDomains)
        {
            IEnumerable<indicator_metadata> indicators;
            //if the aggregate is null then the linq expression need the null keyword
            if (aggregate == null)
            {
                if (returnDomains)
                {
                    indicators =
                    (from i in db.indicator_metadata.Where(x => x.cat1 == category
                        && x.cat2 == subCategory
                        && x.cat3 == null
                        && x.published == true
                        && x.genRaster == true)
                     select (i));
                }
                else {
                    indicators =
                        (from i in db.indicator_metadata.Where(x => x.cat1 == category
                            && x.cat2 == subCategory
                            && x.cat3 == null
                            && x.published == true
                            && x.genRaster == true
                            && x.isDomain == false)
                         select (i));
                }
                
            }//if not null then use the variable
            else
            {
                if (returnDomains)
                {
                    indicators =
                        (from i in db.indicator_metadata.Where(x => x.cat1 == category
                            && x.cat2 == subCategory
                            && x.cat3 == aggregate
                            && x.published == true
                            && x.genRaster == true)
                         select (i));
                }
                else {
                    indicators =
                           (from i in db.indicator_metadata.Where(x => x.cat1 == category
                               && x.cat2 == subCategory
                               && x.cat3 == aggregate
                               && x.published == true
                               && x.genRaster == true
                               && x.isDomain == false)
                            select (i));
                }
            }

            return indicators;

            ////Get all the categories
            //IEnumerable<string> categories = AllCategories();

            ////Check to make sure incoming category exists
            //if (categories.Contains(category))
            //{
            //    //Get the sub-categories for the category
            //    IEnumerable<string> subcategories = SubCategoriesByCategory(category);

            //    //Check to make sure the incoming subCategory is valid
            //    if (subcategories.Contains(subCategory))
            //    {
            //        //Get the aggreagates for the subCategory
            //        IEnumerable<string> aggregates = AggregatesBySubCategory(category, subCategory);

            //        //Check to make sure the incoming aggregate is valid
            //        if (aggregates.Contains(aggregate))
            //        {

            //        }
            //        else //the incoming aggregate doesn't exist
            //            throw Exceptions.NotFound("Invalid aggregate (cat3). '" + aggregate + "' is not a valid value.");
            //    }
            //    else //the incoming subcategory doesn't exist
            //        throw Exceptions.NotFound("Invalid subCategory (cat2). '" + subCategory + "' is not a valid value.");
            //}
            //else //the incoming category doesn't exist
            //    throw Exceptions.NotFound("Invalid category (cat1). '" + category + "' is not a valid value.");            
        }
    }
}
