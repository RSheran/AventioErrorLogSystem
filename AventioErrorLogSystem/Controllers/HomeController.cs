using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AventioErrorLogSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

        public ActionResult WelcomeIndex()
        {
            return View();
        }

        public ActionResult AboutProduct()
        {
            return View();
        }

        public ActionResult AboutDevelopers()
        {
            return View();
        }

        public ActionResult Testimonials()
        {
            return View();
        }
    }
}