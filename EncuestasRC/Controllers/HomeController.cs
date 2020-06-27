using EncuestasRC.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EncuestasRC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                int b = 0;
                int x = b;
            }
            catch(Exception ex) {
                Helper.SendExceptionToSentry(ex);
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}