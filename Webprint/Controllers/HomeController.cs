using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webprint.Models;

namespace Webprint.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Jobsprint()
        {
            var service = new PrintJobService();
            var jobs = service.GetActivePrintJobs();
            return View(jobs); // 輸出到 Razor View
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}