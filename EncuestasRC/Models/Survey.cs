//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EncuestasRC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Survey
    {
        public int Id { get; set; }
        public int SurveyYear { get; set; }
        public int SurveyMonth { get; set; }
        public string Comments { get; set; }
        public bool Active { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
    }
}