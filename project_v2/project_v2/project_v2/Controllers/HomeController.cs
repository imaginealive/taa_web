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
        private IRankService rankSvc;
        private IMembershipService membershipSvc;
        public HomeController(IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IDistributedCache _cache,
            IRankService rankSvc,
            IMembershipService membershipSvc)
        {
            cache = _cache;
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
            this.rankSvc = rankSvc;
            this.membershipSvc = membershipSvc;
        }

        public IActionResult Index()
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));

            var model = projectSvc.GetProjects(ViewBag.User._id);
            var ranks = rankSvc.GetAllRank();
            var memberships = Enumerable.Empty<ProjectMembershipModel>();
            foreach (var item in model) memberships = membershipSvc.GetAllProjectMember(item._id);

            var members = ViewBag.User != null ? memberships.Where(it => it.Account_id == ViewBag.User._id && !it.RemoveDate.HasValue) : null;
            var GraphData = new List<ProjectGraphModel>() { };
            foreach (var project in model)
            {
                var data = new ProjectGraphModel() { ProjectId = project._id, ProjectName = project.ProjectName };
                List<FeatureModel> features = featureSvc.GetFeatures(project._id);
                var member = members.FirstOrDefault(it => it.Project_id == project._id);
                var CanSeeAllWork = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanSeeAllWork) || member.CanSeeAllWork : false;
                var AllWork = 0;
                var WorkDone = 0;

                foreach (var feature in features)
                {
                    var ownerThisFeatureWork = false;
                    var BeAssignedThisFeature = members.Any(it => it.Account_id == feature.BeAssignedMember_id && it.Project_id == feature.Project_id);

                    ownerThisFeatureWork = BeAssignedThisFeature || CanSeeAllWork;

                    var stories = storySvc.GetStories(feature._id);
                    foreach (var story in stories)
                    {
                        var ownerThisStoryWork = false;
                        ownerThisStoryWork = members.Any(it => it.Account_id == story.BeAssignedMember_id) || CanSeeAllWork || ownerThisFeatureWork;

                        var tasks = taskSvc.GetTasks(story._id);
                        foreach (var task in tasks)
                        {
                            var ownerThisTaskWork = false;
                            ownerThisTaskWork = members.Any(it => it.Account_id == task.BeAssignedMember_id) || CanSeeAllWork || ownerThisStoryWork;

                            if (ownerThisTaskWork)
                            {
                                AllWork++;
                                if (task.WorkDoneDate.HasValue) WorkDone++;
                            }
                        }

                        if (ownerThisStoryWork)
                        {
                            AllWork++;
                            if (story.WorkDoneDate.HasValue) WorkDone++;
                        }
                    }
                    if (ownerThisFeatureWork)
                    {
                        AllWork++;
                        if (feature.WorkDoneDate.HasValue) WorkDone++;
                    }
                }
                data.WorkDone = WorkDone;
                data.WorkProcess = AllWork;
                GraphData.Add(data);
            }

            ViewBag.CanCreateProject = ViewBag.User.ProjectCreatable;
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
