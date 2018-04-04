using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class HomeController : Controller
    {
        private const string Username = "demo@gmail.com";

        private IProjectService projectSvc;
        public HomeController(IProjectService projectSvc)
        {
            this.projectSvc = projectSvc;
        }

        public IActionResult Index()
        {
            var model = projectSvc.GetProjects(Username);
            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
