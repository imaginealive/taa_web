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
        private IFeatureService featureSvc;
        private IStoryService storySvc;
        private ITaskService taskSvc;
        public HomeController(IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc)
        {
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
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
            
            var GraphData = new List<ProjectGraphModel>() { };
            foreach (var project in model)
            {
                var data = new ProjectGraphModel()
                {
                    ProjectId = project._id,
                    ProjectName = project.ProjectName
                };
                var features = featureSvc.GetFeatures(project._id);
                var AllWork = 0;
                var WorkDone = 0;
                foreach (var feature in features)
                {
                    var stories = storySvc.GetStories(feature._id);
                    foreach (var story in stories)
                    {
                        var tasks = taskSvc.GetTasks(story._id);
                        AllWork = AllWork + tasks.Count();
                        WorkDone = WorkDone + tasks.Where(it => it.WorkDoneDate.HasValue).Count();

                        if (story.WorkDoneDate.HasValue)
                        {
                            WorkDone++;
                        }
                        AllWork++;
                    }
                    if (feature.WorkDoneDate.HasValue)
                    {
                        WorkDone++;
                    }
                    AllWork++;
                }
                data.WorkDone = WorkDone;
                data.WorkProcess = AllWork - WorkDone;
                GraphData.Add(data);
            }
            ViewBag.Data = GraphData;
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
