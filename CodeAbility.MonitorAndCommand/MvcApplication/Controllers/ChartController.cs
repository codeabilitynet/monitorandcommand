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
    public class ChartController : Controller
    {
        static IMessageRepository messageRepository = new SqlMessageRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);
        //static IMessageRepository messageRepository = new AzureMessageRepository(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
        //static ILogEntryRepository logEntryRepository = new SqlLogEntryRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString); 

        public ChartController()
        {

        }

        public ActionResult Index()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel(Average.ChartSpans.Last48Hours);
            chartsViewModel.Load();

            return View(chartsViewModel);
        }

        public ActionResult Last7Days()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel(Average.ChartSpans.Last7Days);
            chartsViewModel.Load();

            return View("Index", chartsViewModel);
        }

        public ActionResult Last30Days()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel(Average.ChartSpans.Last30Days);
            chartsViewModel.Load();

            return View("Index", chartsViewModel);
        }

        public ActionResult Last3Monthes()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel(Average.ChartSpans.Last3Monthes);
            chartsViewModel.Load();

            return View("Index", chartsViewModel);
        }

        public ActionResult LastYear()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel(Average.ChartSpans.LastYear);
            chartsViewModel.Load();

            return View("Index", chartsViewModel);
        }
    }
}
