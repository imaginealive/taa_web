using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IProjectService projectSvc;
        public HomeController(IProjectService projectSvc)
        {
            this.projectSvc = projectSvc;
        }

        public IActionResult Index()
        {
            //Check, Did user login?
            var isLogin = HttpContext.User.Identity.IsAuthenticated;
            if (!isLogin) return RedirectToAction("Login", "Account");
            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var user = JsonConvert.DeserializeObject<AccountModel>(userString);
            ViewBag.User = user;

            var model = projectSvc.GetProjects(user._id);
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
