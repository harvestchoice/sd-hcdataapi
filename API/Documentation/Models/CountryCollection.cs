using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HarvestChoiceApi.Models
{
    /// <summary>
    /// Class used to format the web response of the <see cref="CountryCollectionController"/>
    /// GetCountryCollections() method.
    /// </summary>
    public class CountryCollection
    {        
        public string name { get; set; }
        public string label { get; set; }
        public string code { get; set; }
        public string group { get; set; }
        public string[] ISOs { get; set; }
    }
}