using LinqKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Web;

namespace HarvestChoiceApi.Models
{
   
    public class PostCellValueRequest
    {
        public int[] indicatorIds { get; set; }
        public int[] domainIds { get; set; }
        public string[] countryIds { get; set; }
        public string wktGeometry { get; set; }
        public int pivotDomainId { get; set; }
        public bool returnGeometry { get; set; }
        public bool groupCountry { get; set; }
        public bool doMarketAnalysis { get; set; }
        public string[] regionNames { get; set; }
    }

    public class CellValues
    {
        public CellValues()
        {
            this.ColumnList = new List<Columns>();
            this.ValueList = new ArrayList();
        }

        public List<Columns> ColumnList { get; set; }
        public ArrayList ValueList { get; set; }
    }

    public class Columns
    {
        public Columns()
        { }

        public Columns(string columnName, string columnIndex)
        {
            this.ColumnName = columnName;
            this.ColumnIndex = columnIndex;
        }

        public Columns(string columnName, string columnIndex, string valueType)
        {
            this.ColumnName = columnName;
            this.ColumnIndex = columnIndex;
            this.ColumnDataType = valueType;
        }

        public Columns(string columnName, string columnIndex, string valueType, string sortColumnIndex)
        {
            this.ColumnName = columnName;
            this.ColumnIndex = columnIndex;
            this.ColumnDataType = valueType;
            this.SortColumnIndex = sortColumnIndex;
        }

        public string ColumnName { get; set; }
        public string ColumnDesc { get; set; }
        public string ColumnIndex { get; set; }
        public string ColumnDataType { get; set; }
        public string SortColumnIndex { get; set; }
    }
    
    /// <summary>
    /// Extention of the Entity Framework create class of the CELL_VALUES table.
    /// </summary>
    public partial class CELL_VALUES
    {
        //public static Expression<Func<CELL_VALUES, bool>> ContainsCountries(
        //    params string[] keywords)
        //{
        //    var predicate = PredicateBuilder.False<CELL_VALUES>();

        //    foreach (string keyword in keywords)
        //    {
        //        string temp = keyword;
        //        predicate = predicate.Or(p => p.ISO3.Contains(temp));
        //    }

        //    return predicate;
        //}

        //public static Expression<Func<CELL_VALUES, bool>> ContainsAEZ(
        //        params double[] keyvalues)
        //{
        //    var predicate = PredicateBuilder.False<CELL_VALUES>();

        //    foreach (double keyvalue in keyvalues)
        //    {
        //        double temp = keyvalue;
        //        predicate = predicate.Or(p => p.AEZ_CODE == temp);
        //    }
        //    return predicate;
        //}

        //public static Expression<Func<CELL_VALUES, bool>> ContainsFarmingSystems(
        //       params double[] keyvalues)
        //{
        //    var predicate = PredicateBuilder.False<CELL_VALUES>();

        //    foreach (double keyvalue in keyvalues)
        //    {
        //        double temp = keyvalue;
        //        predicate = predicate.Or(p => p.FS_CODE == temp);
        //    }
        //    return predicate;
        //}

      
    }

    
}