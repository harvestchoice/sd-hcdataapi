using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HarvestChoiceApi.Models
{
    /// <summary>
    /// Country class to hold the country names (ISO3).
    /// </summary>
    public class Country
    {
        public string Name { get; set; }
    }
    /// <summary>
    /// Region class to hold the region names (GAUL1).
    /// </summary>
    public class Region
    {
        public string Name { get; set; }
    }    
    /// <summary>
    /// District class to hold the district names (GAUL2).
    /// </summary>
    public class District
    {
        public string Name { get; set; }
    }
    /// <summary>
    /// RegionDistrict class to hold district names (GAUL2) by region (GAUL1).
    /// </summary>
    public class RegionDistrict
    {
        public string Name { get; set; }
        public List<District> Districts { get; set; }
    }
    /// <summary>
    /// CountryRegion class to hold country names (ISO) by region (GAUL1).
    /// </summary>
    public class CountryRegion
    {
        public string Name { get; set; }
        public List<Region> Regions { get; set; }
    }
}