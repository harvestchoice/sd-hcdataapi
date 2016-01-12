using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
    
namespace HarvestChoiceApi.Models
{
    /// <summary>
    /// Category class to hold the nested structure of indicators grouped
    /// by category.
    /// </summary>
    public class Category
    {
        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }

    public class Subcategory
    {
        public string Name { get; set; }
        public List<Aggregate> Aggregates { get; set; }
    }

    public class Aggregate
    {
        public string Name { get; set; }
        public List<Indicator> Indicators { get; set; }
    }

    public class Indicator
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        public string ColumnName { get; set; }
        public string MicroLabel { get; set; }
        public string ShortLabel { get; set; }
        public string FullLabel { get; set; }
        public string Unit { get; set; }
        public int? Year { get; set; }
        public int? EndYear { get; set; }
        public int? DecimalPlaces { get; set; }
        public string ClassificationType { get; set; }
        public string AggType { get; set; }
        public string AggFormula { get; set; }
        public string LongDescription { get; set; }
        public string Source { get; set; }
        public string DataWorker { get; set; }
    }
}