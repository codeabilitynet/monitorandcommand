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
    public class MessageController : Controller
    {
        static IMessageRepository messageRepository = new SqlMessageRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);
        //static IMessageRepository messageRepository = new AzureMessageRepository(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
        //static ILogEntryRepository logEntryRepository = new SqlLogEntryRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString); 

        const int NUMBER_OF_MESSAGES = 100;
        public MessageController()
        {

        }

        const int REFRESH_PERIOD_IN_MILLISECONDS = 3000;

        #region Messages View

        public ActionResult Index()
        {
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            IEnumerable<Message> lastMessages = GetLastMessages();

            ViewBag.RefreshPeriod = REFRESH_PERIOD_IN_MILLISECONDS; 

            return View(lastMessages);
        }

        //[OutputCache(NoStore = true, Location = OutputCacheLocation.Client, Duration = 60)]
        public ActionResult RefreshMessagePartial()
        {
            IEnumerable<Message> lastMessages = GetLastMessages();

            return PartialView("_MessagePartial", lastMessages);
        }

        public ActionResult Purge()
        {
            messageRepository.Purge();

            return RedirectToAction("Index");
        }

        #endregion 

        #region Helpers 

        protected IEnumerable<Message> GetLastMessages()
        {
            IEnumerable<Message> lastMessages = messageRepository.ListLastMessages(NUMBER_OF_MESSAGES);
            return lastMessages;
        }

        #endregion 
    }
}
