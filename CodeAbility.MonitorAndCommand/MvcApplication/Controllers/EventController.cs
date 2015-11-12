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
    public class EventController : Controller
    {
        static IEventRepository eventRepository = new SqlEventRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);

        const int NUMBER_OF_EVENTS = 100;

        public EventController()
        {

        }

        const int REFRESH_PERIOD_IN_MILLISECONDS = 60000;

        public ActionResult Index()
        {
            IEnumerable<Event> lastEvents = GetLastEvents();

            ViewBag.RefreshPeriod = REFRESH_PERIOD_IN_MILLISECONDS;

            return View(lastEvents);
        }

        public ActionResult Purge()
        {
            eventRepository.Purge();

            return RedirectToAction("Index");
        }

        #region Helpers 

        protected IEnumerable<Event> GetLastEvents()
        {
            IEnumerable<Event> lastEvents = eventRepository.ListLastEvents(NUMBER_OF_EVENTS);
            return lastEvents;
        }

        #endregion 
    }
}
