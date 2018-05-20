using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private IProjectService projectSvc;
        private IFeatureService featureSvc;
        private IStoryService storySvc;
        private ITaskService taskSvc;
        private IMembershipService membershipSvc;
        private IAccountService accountSvc;
        private IRankService rankSvc;
        private IServiceConfigurations serviceConfig;

        public ProjectController(IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IMembershipService membershipSvc,
            IAccountService accountSvc,
            IRankService rankSvc,
            IServiceConfigurations serviceConfig)
        {
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
            this.membershipSvc = membershipSvc;
            this.accountSvc = accountSvc;
            this.rankSvc = rankSvc;
            this.serviceConfig = serviceConfig;
        }

        #region Projects

        public IActionResult Index(string projectid)
        {
            var project = projectSvc.GetProject(projectid);
            var allAcc = accountSvc.GetAllAccount();
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var features = featureSvc.GetFeatures(projectid);
            var ranks = rankSvc.GetAllRank();

            var displayMemberships = new List<DisplayMembership>();
            foreach (var item in allAcc)
            {
                var membership = memberships.FirstOrDefault(it => it.Account_id == item._id);
                var rankName = string.Empty;

                if (membership != null)
                {
                    var rank = ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id);
                    rankName = rank != null ? rank.RankName : string.Empty;

                    var model = new DisplayMembership(membership)
                    {
                        AccountName = $"{item.FirstName} {item.LastName}",
                        Email = item.Email,
                        RankName = rankName
                    };

                    displayMemberships.Add(model);
                }
            }

            var isLogin = HttpContext.User.Identity.IsAuthenticated;
            if (!isLogin) return RedirectToAction("Login", "Account");

            // Check current user permission
            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var user = JsonConvert.DeserializeObject<AccountModel>(userString);
            var currentUser = allAcc.FirstOrDefault(it => it._id == user._id);
            var member = currentUser != null ? memberships.FirstOrDefault(it => it.Account_id == currentUser._id && !it.RemoveDate.HasValue) : null;

            ViewBag.CurrentUser = currentUser;
            ViewBag.CanCreateFeature = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanCreateFeature) || member.CanCreateFeature : false;
            ViewBag.CanCreateStory = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanCreateStoryUnderSelf) || member.CanCreateStoryUnderSelf : false;
            ViewBag.CanCreateTask = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanCreateTaskUnderSelf) || member.CanCreateTaskUnderSelf : false;
            ViewBag.CanEditAllWork = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanEditAllWork) || member.CanEditAllWork : false;
            ViewBag.CanSeeAllWork = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanSeeAllWork) || member.CanSeeAllWork : false;
            ViewBag.CanCompleteProject = member != null ? (member.ProjectRank_id == serviceConfig.MasterRankId) : false;

            var displayFeatures = new List<DisplayFeatureModel>();
            foreach (var feature in features)
            {
                var MyFeatureWork = false;
                MyFeatureWork = member != null ? (feature.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;
                var feature_CreateByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
                var feature_AssginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
                var feature_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);

                var displayStories = new List<DisplayStoryModel>();
                var stories = storySvc.GetStories(feature._id);
                foreach (var story in stories)
                {
                    var MyStoryWork = false;
                    MyStoryWork = member != null ? (story.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;
                    var story_AssginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
                    var story_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);
                    var story_CreateByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);

                    var displayTasks = new List<DisplayTaskModel>();
                    var tasks = taskSvc.GetTasks(story._id);
                    foreach (var task in tasks)
                    {
                        var MyTaskWork = false;
                        MyTaskWork = member != null ? (task.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;
                        var task_AssginByAccount = allAcc.FirstOrDefault(it => it._id == task.AssginByMember_id);
                        var task_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == task.BeAssignedMember_id);
                        var task_CreateByAccount = allAcc.FirstOrDefault(it => it._id == task.CreateByMember_id);

                        var model_task = new DisplayTaskModel(task)
                        {
                            CreateByMemberName = task_CreateByAccount != null ? $"{task_CreateByAccount.FirstName} {task_CreateByAccount.LastName}" : string.Empty,
                            AssginByMemberName = task_AssginByAccount != null ? $"{task_AssginByAccount.FirstName} {task_AssginByAccount.LastName}" : string.Empty,
                            BeAssignedMemberName = task_BeassginByAccount != null ? $"{task_BeassginByAccount.FirstName} {task_BeassginByAccount.LastName}" : string.Empty,
                        };
                        if (MyTaskWork || MyStoryWork || MyFeatureWork) displayTasks.Add(model_task);
                    }

                    var model_story = new DisplayStoryModel(story)
                    {
                        CreateByMemberName = story_CreateByAccount != null ? $"{story_CreateByAccount.FirstName} {story_CreateByAccount.LastName}" : string.Empty,
                        AssginByMemberName = story_AssginByAccount != null ? $"{story_AssginByAccount.FirstName} {story_AssginByAccount.LastName}" : string.Empty,
                        BeAssignedMemberName = story_BeassginByAccount != null ? $"{story_BeassginByAccount.FirstName} {story_BeassginByAccount.LastName}" : string.Empty,
                        Tasks = displayTasks
                    };
                    if (MyStoryWork || model_story.Tasks.Count() > 0 || MyFeatureWork) displayStories.Add(model_story);
                }

                var model_feature = new DisplayFeatureModel(feature)
                {
                    CreateByMemberName = feature_CreateByAccount != null ? $"{feature_CreateByAccount.FirstName} {feature_CreateByAccount.LastName}" : string.Empty,
                    AssginByMemberName = feature_AssginByAccount != null ? $"{feature_AssginByAccount.FirstName} {feature_AssginByAccount.LastName}" : string.Empty,
                    BeAssignedMemberName = feature_BeassginByAccount != null ? $"{feature_BeassginByAccount.FirstName} {feature_BeassginByAccount.LastName}" : string.Empty,
                    Stories = displayStories
                };
                if (MyFeatureWork || model_feature.Stories.Count() > 0) displayFeatures.Add(model_feature);
            }

            return View(new ProjectDetailModel
            {
                Project = project,
                Memberships = displayMemberships,
                Features = displayFeatures
            });
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
                if (model.ClosingDate.Date < DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างโปรเจคได้ เนื่องจากวันที่เสร็จสิ้นโปรเจค ไม่สามารถน้อยกว่าวันที่ปัจจุบันได้";
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);

                var isLogin = HttpContext.User.Identity.IsAuthenticated;
                if (!isLogin) return RedirectToAction("Login", "Account");

                // Check current user permission
                var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                var user = JsonConvert.DeserializeObject<AccountModel>(userString);
                ViewBag.User = user;

                projectSvc.CreateProject(user._id, model);
                return RedirectToAction(nameof(Index), "Home");
            }
            return View(model);
        }

        public IActionResult Detail(string projectid)
        {
            var accountMemberships = new List<AccountModel>();
            var displayMemberships = new List<DisplayMembership>();

            var project = projectSvc.GetProject(projectid);
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var ranks = rankSvc.GetAllRank();
            var allAcc = accountSvc.GetAllAccount();

            foreach (var item in allAcc)
            {
                if (memberships.FirstOrDefault(it => it.Account_id == item._id && !it.RemoveDate.HasValue) != null)
                {
                    accountMemberships.Add(item);
                }
            }

            foreach (var item in accountMemberships)
            {
                var membership = memberships.FirstOrDefault(it => it.Account_id == item._id);
                if (membership != null)
                {
                    var allWorkHasBeenAssigned = 0;

                    var features = featureSvc.GetFeatures(projectid);
                    foreach (var feature in features)
                    {
                        var stories = storySvc.GetStories(feature._id);
                        foreach (var story in stories)
                        {
                            var tasks = taskSvc.GetTasks(story._id);
                            allWorkHasBeenAssigned += tasks.Where(it => it.BeAssignedMember_id == membership.Account_id).Count();
                        }
                        allWorkHasBeenAssigned += stories.Where(it => it.BeAssignedMember_id == membership.Account_id).Count();
                    }
                    allWorkHasBeenAssigned += features.Where(it => it.BeAssignedMember_id == membership.Account_id).Count();


                    var rankName = ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id).RankName;
                    var model = new DisplayMembership(membership)
                    {
                        AccountName = $"{item.FirstName} {item.LastName}",
                        Email = item.Email,
                        RankName = rankName,
                        AllWorkHasBeenAssigned = allWorkHasBeenAssigned
                    };

                    displayMemberships.Add(model);
                }
            };

            var isLogin = HttpContext.User.Identity.IsAuthenticated;
            if (!isLogin) return RedirectToAction("Login", "Account");

            // Check current user permission
            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var user = JsonConvert.DeserializeObject<AccountModel>(userString);
            var currentUser = allAcc.FirstOrDefault(it => it._id == user._id);
            var member = currentUser != null ? memberships.FirstOrDefault(it => it.Account_id == currentUser._id && !it.RemoveDate.HasValue) : null;

            ViewBag.CanEditProject = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanEditProject) : false;
            ViewBag.CanEditMember = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanManageMember) : false;

            return View(new ProjectDetailModel
            {
                Project = project,
                Memberships = displayMemberships
            });
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
                if (model.ClosingDate.Date < DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถแก้ไขโปรเจคได้ เนื่องจากวันที่เสร็จสิ้นโปรเจค ไม่สามารถน้อยกว่าวันที่ปัจจุบันได้";
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);

                projectSvc.EditProject(model);
                return RedirectToAction(nameof(Detail), new { projectid = model._id });
            }
            return View(model);
        }

        public IActionResult ProjectComplete(string projectid)
        {
            var project = projectSvc.GetProject(projectid);
            project.WorkDoneDate = DateTime.Now;
            projectSvc.EditProject(project);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult Report(string id)
        {
            var allAcc = accountSvc.GetAllAccount();
            var features = featureSvc.GetFeatures(id);
            var project = projectSvc.GetProject(id);
            var memberships = membershipSvc.GetAllProjectMember(project._id);
            var ranks = rankSvc.GetAllRank();

            var isLogin = HttpContext.User.Identity.IsAuthenticated;
            if (!isLogin) return RedirectToAction("Login", "Account");

            // Check current user permission
            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var user = JsonConvert.DeserializeObject<AccountModel>(userString);
            var currentUser = allAcc.FirstOrDefault(it => it._id == user._id);
            var member = currentUser != null ? memberships.FirstOrDefault(it => it.Account_id == currentUser._id && !it.RemoveDate.HasValue) : null;

            ViewBag.ProjectId = project._id;
            ViewBag.ProjectName = project.ProjectName;
            ViewBag.CanSeeAllWork = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanSeeAllWork) || member.CanSeeAllWork : false;

            var model = new List<DisplayFeatureModel>();
            foreach (var feature in features)
            {
                var MyFeatureWork = false;
                MyFeatureWork = member != null ? (feature.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;

                var feature_CreateByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
                var feature_AssginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
                var feature_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);
                var AllStories = new List<DisplayStoryModel>();
                var stories = storySvc.GetStories(feature._id);
                foreach (var story in stories)
                {
                    var MyStoryWork = false;
                    MyStoryWork = member != null ? (story.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;
                    var story_AssginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
                    var story_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);
                    var story_CreateByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);

                    var AllTasks = new List<DisplayTaskModel>();
                    var tasks = taskSvc.GetTasks(story._id);
                    foreach (var task in tasks)
                    {
                        var MyTaskWork = false;
                        MyTaskWork = member != null ? (task.BeAssignedMember_id == member.Account_id || ViewBag.CanSeeAllWork) : false;
                        var task_AssginByAccount = allAcc.FirstOrDefault(it => it._id == task.AssginByMember_id);
                        var task_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == task.BeAssignedMember_id);
                        var task_CreateByAccount = allAcc.FirstOrDefault(it => it._id == task.CreateByMember_id);

                        var model_task = new DisplayTaskModel(task)
                        {
                            CreateByMemberName = task_CreateByAccount != null ? $"{task_CreateByAccount.FirstName} {task_CreateByAccount.LastName}" : "-",
                            AssginByMemberName = task_AssginByAccount != null ? $"{task_AssginByAccount.FirstName} {task_AssginByAccount.LastName}" : "-",
                            BeAssignedMemberName = task_BeassginByAccount != null ? $"{task_BeassginByAccount.FirstName} {task_BeassginByAccount.LastName}" : "-",
                        };
                        if (MyTaskWork || MyStoryWork || MyFeatureWork) AllTasks.Add(model_task);
                    }

                    var model_story = new DisplayStoryModel(story)
                    {
                        CreateByMemberName = story_CreateByAccount != null ? $"{story_CreateByAccount.FirstName} {story_CreateByAccount.LastName}" : "-",
                        AssginByMemberName = story_AssginByAccount != null ? $"{story_AssginByAccount.FirstName} {story_AssginByAccount.LastName}" : "-",
                        BeAssignedMemberName = story_BeassginByAccount != null ? $"{story_BeassginByAccount.FirstName} {story_BeassginByAccount.LastName}" : "-",
                        Tasks = AllTasks
                    };
                    if (MyStoryWork || model_story.Tasks.Count() > 0 || MyFeatureWork) AllStories.Add(model_story);
                }
                var model_feature = new DisplayFeatureModel(feature)
                {
                    CreateByMemberName = feature_CreateByAccount != null ? $"{feature_CreateByAccount.FirstName} {feature_CreateByAccount.LastName}" : "-",
                    AssginByMemberName = feature_AssginByAccount != null ? $"{feature_AssginByAccount.FirstName} {feature_AssginByAccount.LastName}" : "-",
                    BeAssignedMemberName = feature_BeassginByAccount != null ? $"{feature_BeassginByAccount.FirstName} {feature_BeassginByAccount.LastName}" : "-",
                    Stories = AllStories
                };
                if (MyFeatureWork || model_feature.Stories.Count() > 0) model.Add(model_feature);
            }
            return View(model);
        }

        #endregion Projects

        #region Memberships

        public IActionResult AllMemberships(string projectid)
        {
            var nonMemberships = new List<AccountModel>();
            var accountMemberships = new List<AccountModel>();
            var displayMemberships = new List<DisplayMembership>();

            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var allAcc = accountSvc.GetAllAccount();
            var ranks = rankSvc.GetAllRank();

            foreach (var item in allAcc)
            {
                if (memberships.FirstOrDefault(it => it.Account_id == item._id && !it.RemoveDate.HasValue) != null)
                    accountMemberships.Add(item);
                else
                    nonMemberships.Add(item);
            }

            foreach (var item in accountMemberships)
            {
                var membership = memberships.FirstOrDefault(it => it.Account_id == item._id);
                var rankName = ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id).RankName;
                var model = new DisplayMembership(membership)
                {
                    AccountName = $"{item.FirstName} {item.LastName}",
                    Email = item.Email,
                    RankName = rankName
                };

                displayMemberships.Add(model);
            };

            var isLogin = HttpContext.User.Identity.IsAuthenticated;
            if (!isLogin) return RedirectToAction("Login", "Account");

            // Check current user permission
            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var user = JsonConvert.DeserializeObject<AccountModel>(userString);
            ViewBag.CurrentUser = user;

            ViewBag.RankMaster = serviceConfig.MasterRankId;
            return View(new MembershipManagementModel
            {
                ProjectId = projectid,
                Memberships = displayMemberships,
                NonMemberships = nonMemberships
            });
        }

        public IActionResult AddMembership(string projectid, string accountid)
        {
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var membership = memberships.FirstOrDefault(it => it.Account_id == accountid);
            if (membership != null && membership.RemoveDate.HasValue)
            {
                membership.RemoveDate = null;
                membershipSvc.EditMember(membership);
            }
            else
            {
                membershipSvc.AddMember(accountid, projectid);
            }

            return RedirectToAction(nameof(AllMemberships), new { projectid = projectid });
        }

        public IActionResult RemoveMembership(string projectid, string accountid)
        {
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var membership = memberships.FirstOrDefault(it => it.Account_id == accountid);
            membership.RemoveDate = DateTime.Now;

            membershipSvc.EditMember(membership);
            return RedirectToAction(nameof(AllMemberships), new { projectid = projectid });
        }

        public IActionResult ChangeMembershipRank(string projectid, string accountid, string rankid)
        {
            var account = accountSvc.GetAllAccount().First(it => it._id == accountid);
            var ranks = rankSvc.GetAllRank().Where(it => it._id != serviceConfig.MasterRankId).ToList();
            var member = membershipSvc.GetAllProjectMember(projectid).FirstOrDefault(it => it.Account_id == account._id);

            var model = new EditRankModel
            {
                ProjectId = projectid,
                AccountId = accountid,
                Email = account.Email,
                Name = account.AccountName,
                RankId = rankid,
                Ranks = ranks,
                CanSeeAllWork = member.CanSeeAllWork,
                CanEditAllWork = member.CanEditAllWork,
                CanAssign = member.CanAssign,
                BeAssigned = member.BeAssigned,
                CanCreateFeature = member.CanCreateFeature,
                CanCreateStoryUnderSelf = member.CanCreateStoryUnderSelf,
                CanCreateTaskUnderSelf = member.CanCreateTaskUnderSelf
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ChangeMembershipRank(string projectid, string accountid, string rankid, EditRankModel body)
        {
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var membership = memberships.FirstOrDefault(it => it.Account_id == accountid);
            membership.ProjectRank_id = rankid;
            membership.CanSeeAllWork = body.CanSeeAllWork;
            membership.CanEditAllWork = body.CanEditAllWork;
            membership.CanAssign = body.CanAssign;
            membership.BeAssigned = body.BeAssigned;
            membership.CanCreateFeature = body.CanCreateFeature;
            membership.CanCreateStoryUnderSelf = body.CanCreateStoryUnderSelf;
            membership.CanCreateTaskUnderSelf = body.CanCreateTaskUnderSelf;

            membershipSvc.EditMember(membership);
            return RedirectToAction(nameof(AllMemberships), new { projectid = projectid });
        }

        #endregion Memberships
    }
}