using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class ProjectController : Controller
    {
        private IProjectService projectSvc;
        public ProjectController(IProjectService projectSvc)
        {
            this.projectSvc = projectSvc;
        }

        public IActionResult Index(string projectid)
        {
            // TODO: Get membership
            // TODO: Get Works (Features, Stories and Tasks)
            var model = projectSvc.GetProject(projectid);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
                if (isLogin.Length == 0) return RedirectToAction("Login", "Account");

                var json = System.Text.Encoding.UTF8.GetString(isLogin);
                var user = JsonConvert.DeserializeObject<AccountModel>(json);
                ViewBag.User = user;

                projectSvc.CreateProject(user._id, model);
                return RedirectToAction(nameof(Index), "Home");
            }
            return View(model);
        }

        public IActionResult Detail(string projectid)
        {
            // TODO: Get membership
            var model = projectSvc.GetProject(projectid);
            return View(model);
        }

        public IActionResult Edit(string projectid)
        {
            var model = projectSvc.GetProject(projectid);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                projectSvc.EditProject(model);
                return RedirectToAction(nameof(Detail), new { projectid = model._id });
            }
            return View(model);
        }
    }
}