using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AventioErrorLogSystem.Controllers
{
    public class LoginErrorController : Controller
    {
        // GET: LoginError
        public ActionResult Index()
        {
            string errorMsg = System.Web.HttpContext.Current.Session["ErrorMsg"].ToString();

            ViewBag.ErrorMsg = errorMsg.ToLower().Trim();

            return View();
        }
    }
}