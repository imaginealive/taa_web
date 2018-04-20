using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Attribute;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    [LoginSession]
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
            //Check, Did user login?
            var isLogin = HttpContext.Session.GetString("LoginData");
            if (string.IsNullOrEmpty(isLogin)) return RedirectToAction("Login", "Account");

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
