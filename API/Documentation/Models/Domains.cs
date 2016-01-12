using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HarvestChoiceApi.Models
{
    public class Domains
    {
        public Domains() { 
            this.DomainAreas = new List<DomainArea>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public List<DomainArea> DomainAreas { get; set; }

    }

    public class DomainArea
    {
        public DomainArea()
        {
            this.Classifications = new List<Classification>();
        }

        public int Id { get; set; }
        public string ColumnName { get; set; }
        public string MicroLabel { get; set; }
        public string LongDescription { get; set; }
        public string Unit { get; set; }
        public int? Year { get; set; }
        public int? DecimalPlaces { get; set; }
        public string ClassificationType { get; set; }
        public string AggType { get; set; }
        public string AggFormula { get; set; }
        public List<Classification> Classifications { get; set; }
    }

    public class Classification
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public int? SortOrder { get; set; }
    }
}