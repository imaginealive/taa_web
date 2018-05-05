using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Attribute;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    [LoginSession]
    public class ProjectController : Controller
    {
        private IProjectService projectSvc;
        private IFeatureService featureSvc;
        private IStoryService storySvc;
        private ITaskService taskSvc;
        private IMembershipService membershipSvc;
        private IAccountService accountSvc;
        private IRankService rankSvc;
        IServiceConfigurations serviceConfig;
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

            var displayFeatures = new List<DisplayFeatureModel>();
            foreach (var feature in features)
            {
                var feature_CreateByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
                var feature_AssginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
                var feature_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);

                var displayStories = new List<DisplayStoryModel>();
                var stories = storySvc.GetStories(feature._id);
                foreach (var story in stories)
                {
                    var story_AssginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
                    var story_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);
                    var story_CreateByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);

                    var displayTasks = new List<DisplayTaskModel>();
                    var tasks = taskSvc.GetTasks(story._id);
                    foreach (var task in tasks)
                    {
                        var task_AssginByAccount = allAcc.FirstOrDefault(it => it._id == task.AssginByMember_id);
                        var task_BeassginByAccount = allAcc.FirstOrDefault(it => it._id == task.BeAssignedMember_id);
                        var task_CreateByAccount = allAcc.FirstOrDefault(it => it._id == task.CreateByMember_id);

                        var model_task = new DisplayTaskModel(task)
                        {
                            CreateByMemberName = task_AssginByAccount != null ? $"{task_AssginByAccount.FirstName} {task_AssginByAccount.LastName}" : string.Empty,
                            AssginByMemberName = task_BeassginByAccount != null ? $"{task_BeassginByAccount.FirstName} {task_BeassginByAccount.LastName}" : string.Empty,
                            BeAssignedMemberName = task_CreateByAccount != null ? $"{task_CreateByAccount.FirstName} {task_CreateByAccount.LastName}" : string.Empty,
                        };
                        displayTasks.Add(model_task);
                    }

                    var model_story = new DisplayStoryModel(story)
                    {
                        CreateByMemberName = story_CreateByAccount != null ? $"{story_CreateByAccount.FirstName} {story_CreateByAccount.LastName}" : string.Empty,
                        AssginByMemberName = story_AssginByAccount != null ? $"{story_AssginByAccount.FirstName} {story_AssginByAccount.LastName}" : string.Empty,
                        BeAssignedMemberName = story_BeassginByAccount != null ? $"{story_BeassginByAccount.FirstName} {story_BeassginByAccount.LastName}" : string.Empty,
                        Tasks = displayTasks
                    };
                    displayStories.Add(model_story);
                }

                var model_feature = new DisplayFeatureModel(feature)
                {
                    CreateByMemberName = feature_CreateByAccount != null ? $"{feature_CreateByAccount.FirstName} {feature_CreateByAccount.LastName}" : string.Empty,
                    AssginByMemberName = feature_AssginByAccount != null ? $"{feature_AssginByAccount.FirstName} {feature_AssginByAccount.LastName}" : string.Empty,
                    BeAssignedMemberName = feature_BeassginByAccount != null ? $"{feature_BeassginByAccount.FirstName} {feature_BeassginByAccount.LastName}" : string.Empty,
                    Stories = displayStories
                };
                displayFeatures.Add(model_feature);
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
                var rankName = ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id).RankName;
                var model = new DisplayMembership(membership)
                {
                    AccountName = $"{item.FirstName} {item.LastName}",
                    Email = item.Email,
                    RankName = rankName
                };

                displayMemberships.Add(model);
            };

            HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
            if (isLogin.Length == 0) return RedirectToAction("Login", "Account");

            // Check current user permission
            var json = System.Text.Encoding.UTF8.GetString(isLogin);
            var user = JsonConvert.DeserializeObject<AccountModel>(json);
            var currentUser = allAcc.FirstOrDefault(it => it._id == user._id);
            var member = currentUser != null ? memberships.FirstOrDefault(it => it.Account_id == currentUser._id && !it.RemoveDate.HasValue) : null;

            ViewBag.CanEditProject = member != null ?
                (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanEditProject ||
                currentUser.IsAdmin ||
                currentUser.ProjectCreatable) : false;
            ViewBag.CanEditMember = member != null ?
                (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanManageMember ||
                currentUser.IsAdmin ||
                currentUser.ProjectCreatable) : false;

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
            var model = new EditRankModel
            {
                ProjectId = projectid,
                AccountId = accountid,
                Email = account.Email,
                Name = account.AccountName,
                RankId = rankid,
                Ranks = ranks
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ChangeMembershipRank(string projectid, string accountid, string rankid, EditRankModel body)
        {
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var membership = memberships.FirstOrDefault(it => it.Account_id == accountid);
            membership.ProjectRank_id = rankid;

            membershipSvc.EditMember(membership);
            return RedirectToAction(nameof(AllMemberships), new { projectid = projectid });
        }

        #endregion Memberships
    }
}