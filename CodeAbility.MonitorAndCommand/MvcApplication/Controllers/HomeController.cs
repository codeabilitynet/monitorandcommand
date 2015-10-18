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

using System.Web.UI;

namespace MvcApplication.Controllers
{
    public class HomeController : Controller
    {
        static IMessageRepository messageRepository = new SqlMessageRepository(ConfigurationManager.ConnectionStrings["MonitorAndCommand"].ConnectionString);
        //static IMessageRepository messageRepository = new AzureMessageRepository(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        public HomeController()
        {

        }

        public ActionResult Index()
        {
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            IEnumerable<Message> lastMessages = messageRepository.ListMessages();

            return View(lastMessages);
        }

        [OutputCache(NoStore = true, Location = OutputCacheLocation.Client, Duration = 3)]
        public ActionResult RefreshMessagePartial()
        {
            IEnumerable<Message> lastMessages = messageRepository.ListMessages();

            return PartialView("_MessagePartial", lastMessages);
        }

        public ActionResult Purge()
        {
            //ViewBag.Message = "Your app description page.";

            messageRepository.Purge();

            return RedirectToAction("Index");
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
