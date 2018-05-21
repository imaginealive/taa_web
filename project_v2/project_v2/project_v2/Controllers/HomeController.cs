using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class HomeController : Controller
    {
        private IProjectService projectSvc;
        private IFeatureService featureSvc;
        private IStoryService storySvc;
        private ITaskService taskSvc;
        public IDistributedCache cache;
        public HomeController(IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IDistributedCache _cache)
        {
            cache = _cache;
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
        }

        public IActionResult Index()
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");
            
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));

            var model = projectSvc.GetProjects(ViewBag.User._id);

            var GraphData = new List<ProjectGraphModel>() { };
            foreach (var project in model)
            {
                var data = new ProjectGraphModel()
                {
                    ProjectId = project._id,
                    ProjectName = project.ProjectName
                };
                List<FeatureModel> features = featureSvc.GetFeatures(project._id);
                var AllWork = 0;
                var WorkDone = 0;
                foreach (var feature in features)
                {
                    List<StoryModel> stories = storySvc.GetStories(feature._id);
                    foreach (var story in stories)
                    {
                        List<TaskModel> tasks = taskSvc.GetTasks(story._id);
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
