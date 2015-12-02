using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Repository;
using CodeAbility.MonitorAndCommand.SqlStorage;
using CodeAbility.MonitorAndCommand.AzureStorage;

using MvcApplication.ViewModels; 

using System.Web.UI;

namespace MvcApplication.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {

        }

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
