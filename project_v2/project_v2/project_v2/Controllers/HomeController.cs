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
        private IAccountService accountSvc;
        public IDistributedCache cache;
        private IRankService rankSvc;
        private IMembershipService membershipSvc;
        public HomeController(IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IAccountService accountSvc,
            IDistributedCache _cache,
            IRankService rankSvc,
            IMembershipService membershipSvc)
        {
            cache = _cache;
            this.accountSvc = accountSvc;
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

            var allAcc = accountSvc.GetAllAccount();
            var model = projectSvc.GetProjects(ViewBag.User._id);
            var ranks = rankSvc.GetAllRank();

            var GraphData = new List<ProjectGraphModel>() { };
            foreach (var project in model)
            {
                List<ProjectMembershipModel> memberships = membershipSvc.GetAllProjectMember(project._id);
                var members = memberships.Where(it => it.Account_id == ViewBag.User._id && !it.RemoveDate.HasValue);
                var data = new ProjectGraphModel() { ProjectId = project._id, ProjectName = project.ProjectName };
                List<FeatureModel> features = featureSvc.GetFeatures(project._id);
                var member = members.FirstOrDefault(it => it.Project_id == project._id);
                var CanSeeAllWork = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanSeeAllWork) || member.CanSeeAllWork : false;
                //if (true)
                //{
                //    data.WorkDone = features.Where(it => it.WorkDoneDate.HasValue).Count() + features.Sum(it => storySvc.GetStories(it._id).Where(st => st.WorkDoneDate.HasValue).Count()) + features.Sum(it => storySvc.GetStories(it._id).Sum(st => taskSvc.GetTasks(st._id).Where(t => t.WorkDoneDate.HasValue).Count()));
                //    data.WorkProcess = features.Where(it => !it.WorkDoneDate.HasValue).Count() + features.Sum(it => storySvc.GetStories(it._id).Where(st => !st.WorkDoneDate.HasValue).Count()) + features.Sum(it => storySvc.GetStories(it._id).Sum(st => taskSvc.GetTasks(st._id).Where(t => !t.WorkDoneDate.HasValue).Count()));
                //    GraphData.Add(data);

                //}
                //else
                //{
                var Features = new List<DisplayFeatureModel>();
                var displayFeatures = new List<DisplayFeatureModel>();
                foreach (var feature in features)
                {
                    var MyFeatureWork = false;
                    MyFeatureWork = member != null ? (feature.BeAssignedMember_id == member.Account_id || CanSeeAllWork) : false;

                    var feature_CreateByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
                    var feature_AssginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
                    var feature_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);
                    var Stories = new List<DisplayStoryModel>();
                    var displayStories = new List<DisplayStoryModel>();
                    var stories = storySvc.GetStories(feature._id);
                    foreach (var story in stories)
                    {
                        var MyStoryWork = false;
                        MyStoryWork = member != null ? (story.BeAssignedMember_id == member.Account_id || CanSeeAllWork) : false;
                        var story_AssginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
                        var story_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);
                        var story_CreateByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);
                        var Tasks = new List<DisplayTaskModel>();
                        var displayTasks = new List<DisplayTaskModel>();
                        var tasks = taskSvc.GetTasks(story._id);
                        foreach (var task in tasks)
                        {
                            var MyTaskWork = false;
                            MyTaskWork = member != null ? (task.BeAssignedMember_id == member.Account_id || CanSeeAllWork) : false;
                            var task_AssginByAccount = allAcc.FirstOrDefault(it => it._id == task.AssginByMember_id);
                            var task_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == task.BeAssignedMember_id);
                            var task_CreateByAccount = allAcc.FirstOrDefault(it => it._id == task.CreateByMember_id);

                            var model_task = new DisplayTaskModel(task)
                            {
                                CreateByMemberName = task_CreateByAccount != null ? $"{task_CreateByAccount.FirstName} {task_CreateByAccount.LastName}" : "-",
                                AssginByMemberName = task_AssginByAccount != null ? $"{task_AssginByAccount.FirstName} {task_AssginByAccount.LastName}" : "-",
                                BeAssignedMemberName = task_BeassginByAccount != null ? $"{task_BeassginByAccount.FirstName} {task_BeassginByAccount.LastName}" : "-",
                            };
                            Tasks.Add(model_task);
                            if (MyTaskWork || MyStoryWork || MyFeatureWork) displayTasks.Add(model_task);
                        }

                        var model_storyAllTask = new DisplayStoryModel(story)
                        {
                            CreateByMemberName = story_CreateByAccount != null ? $"{story_CreateByAccount.FirstName} {story_CreateByAccount.LastName}" : "-",
                            AssginByMemberName = story_AssginByAccount != null ? $"{story_AssginByAccount.FirstName} {story_AssginByAccount.LastName}" : "-",
                            BeAssignedMemberName = story_BeassginByAccount != null ? $"{story_BeassginByAccount.FirstName} {story_BeassginByAccount.LastName}" : "-",
                            Tasks = Tasks
                        };
                        var model_story = new DisplayStoryModel(story)
                        {
                            CreateByMemberName = story_CreateByAccount != null ? $"{story_CreateByAccount.FirstName} {story_CreateByAccount.LastName}" : "-",
                            AssginByMemberName = story_AssginByAccount != null ? $"{story_AssginByAccount.FirstName} {story_AssginByAccount.LastName}" : "-",
                            BeAssignedMemberName = story_BeassginByAccount != null ? $"{story_BeassginByAccount.FirstName} {story_BeassginByAccount.LastName}" : "-",
                            Tasks = displayTasks
                        };
                        Stories.Add(model_storyAllTask);
                        if (MyStoryWork || model_story.Tasks.Count() > 0 || MyFeatureWork) displayStories.Add(model_story);
                    }
                    var model_featureAllStory = new DisplayFeatureModel(feature)
                    {
                        CreateByMemberName = feature_CreateByAccount != null ? $"{feature_CreateByAccount.FirstName} {feature_CreateByAccount.LastName}" : "-",
                        AssginByMemberName = feature_AssginByAccount != null ? $"{feature_AssginByAccount.FirstName} {feature_AssginByAccount.LastName}" : "-",
                        BeAssignedMemberName = feature_BeassginByAccount != null ? $"{feature_BeassginByAccount.FirstName} {feature_BeassginByAccount.LastName}" : "-",
                        Stories = Stories
                    };
                    var model_feature = new DisplayFeatureModel(feature)
                    {
                        CreateByMemberName = feature_CreateByAccount != null ? $"{feature_CreateByAccount.FirstName} {feature_CreateByAccount.LastName}" : "-",
                        AssginByMemberName = feature_AssginByAccount != null ? $"{feature_AssginByAccount.FirstName} {feature_AssginByAccount.LastName}" : "-",
                        BeAssignedMemberName = feature_BeassginByAccount != null ? $"{feature_BeassginByAccount.FirstName} {feature_BeassginByAccount.LastName}" : "-",
                        Stories = displayStories
                    };
                    Features.Add(model_featureAllStory);
                    if (MyFeatureWork || model_feature.Stories.Count() > 0) displayFeatures.Add(model_feature);
                }
                if (CanSeeAllWork)
                {
                    data.WorkDone = Features.Where(it => it.WorkDoneDate.HasValue).Count() + Features.Sum(it => it.Stories.Where(st => st.WorkDoneDate.HasValue).Count()) + Features.Sum(it => it.Stories.Sum(st => st.Tasks.Where(t => t.WorkDoneDate.HasValue).Count()));
                    data.WorkProcess = Features.Where(it => !it.WorkDoneDate.HasValue).Count() + Features.Sum(it => it.Stories.Where(st => !st.WorkDoneDate.HasValue).Count()) + Features.Sum(it => it.Stories.Sum(st => st.Tasks.Where(t => !t.WorkDoneDate.HasValue).Count()));
                }
                else
                {
                    data.WorkDone = displayFeatures.Where(it => it.WorkDoneDate.HasValue).Count() + displayFeatures.Sum(it => it.Stories.Where(st => st.WorkDoneDate.HasValue).Count()) + displayFeatures.Sum(it => it.Stories.Sum(st => st.Tasks.Where(t => t.WorkDoneDate.HasValue).Count()));
                    data.WorkProcess = displayFeatures.Where(it => !it.WorkDoneDate.HasValue).Count() + displayFeatures.Sum(it => it.Stories.Where(st => !st.WorkDoneDate.HasValue).Count()) + displayFeatures.Sum(it => it.Stories.Sum(st => st.Tasks.Where(t => !t.WorkDoneDate.HasValue).Count()));
                }
                GraphData.Add(data);
                //}
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
