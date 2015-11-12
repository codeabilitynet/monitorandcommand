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

        const int NUMBER_OF_MESSAGES = 100;

        public ChartController()
        {

        }

        const int REFRESH_PERIOD_IN_MILLISECONDS = 10000;

        const int DEFAULT_ROW_INTERVAL = 10;
        const int HOURLY_ROW_INTERVAL = 60; 

        public ActionResult Index()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel();
            chartsViewModel.Load(DEFAULT_ROW_INTERVAL); 

            ViewBag.RefreshPeriod = REFRESH_PERIOD_IN_MILLISECONDS;

            return View(chartsViewModel);
        }

        public ActionResult ShowHourlyView()
        {
            ViewBag.Message = "Charts.";

            ChartsViewModel chartsViewModel = new ChartsViewModel();
            chartsViewModel.Load(HOURLY_ROW_INTERVAL);

            ViewBag.RefreshPeriod = REFRESH_PERIOD_IN_MILLISECONDS;

            return View(chartsViewModel);
        }

        //[OutputCache(NoStore = true, Location = OutputCacheLocation.Client, Duration = 60)]
        public ActionResult RefreshChartPartial()
        {
            ChartsViewModel chartsViewModel = new ChartsViewModel();
            chartsViewModel.Load(DEFAULT_ROW_INTERVAL); 

            return PartialView("_ChartPartial", chartsViewModel);
        }
    }
}
