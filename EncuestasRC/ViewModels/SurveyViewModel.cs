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

    }
}