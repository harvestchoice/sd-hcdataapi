//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HarvestChoiceApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class continuous_classification
    {
        public int id { get; set; }
        public int domainid { get; set; }
        public Nullable<double> min { get; set; }
        public Nullable<double> max { get; set; }
        public int classid { get; set; }
        public Nullable<int> sortorder { get; set; }
    
        public virtual classification classification { get; set; }
        public virtual domain_variable domain_variable { get; set; }
    }
}