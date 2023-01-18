using EncuestasRC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncuestasRC.ViewModels
{
    public class SurveyViewModel
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public int SurveyYear { get; set; }
        public int SurveyMonth { get; set; }
        public string Comments { get; set; }
        public bool Active { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        
        public int Completed { get; set; }
        public decimal Result { get; set; }
    }

    public class SurveyCompletedViewModel
    {
        public string Title { get; set; }
        public SurveyHeader surveyHeader { get; set; }
        public List<SurveyDetail> surveyDetail { get; set; }
    }

    public class SurveyListViewModel
    {
        public int Id { get; set; }
        public DateTime? SurveyEnded { get; set; }
        public string Customer { get; set; }
        public int? CustomerType { get; set; }
        public string OrderNo { get; set; }
        public decimal? Result { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CloseDate { get; set; }
    }
}