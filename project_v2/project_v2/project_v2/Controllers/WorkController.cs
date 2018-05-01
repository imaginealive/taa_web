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
    public class WorkController : Controller
    {
        private IProjectService projectSvc;
        private IFeatureService featureSvc;
        private IStoryService storySvc;
        private ITaskService taskSvc;
        private IMembershipService membershipSvc;
        private IAccountService accountSvc;
        private IRankService rankSvc;
        private IStatusService statusSvc;
        public WorkController(
            IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IMembershipService membershipSvc,
            IAccountService accountSvc,
            IRankService rankSvc,
            IStatusService statusSvc)
        {
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
            this.membershipSvc = membershipSvc;
            this.accountSvc = accountSvc;
            this.rankSvc = rankSvc;
            this.statusSvc = statusSvc;
        }

        #region Features

        public IActionResult CreateFeature(string projectid)
        {
            PrepareDataForDisplay(projectid);
            return View(new FeatureModel { Project_id = projectid });
        }

        [HttpPost]
        public IActionResult CreateFeature(FeatureModel model)
        {
            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(model.Project_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date < DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานหลักได้ เนื่องจากวันที่เสร็จสิ้นงานหลัก ไม่สามารถน้อยกว่าวันที่ปัจจุบันได้ หรือไม่สามารถมากกว่าวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(model.Project_id);
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);
                if (!string.IsNullOrEmpty(model.BeAssignedMember_id)) model.AssginByMember_id = model.CreateByMember_id;

                featureSvc.CreateFeature(model);
                return RedirectToAction("Index", "Project", new { projectid = model.Project_id });
            }
            return View(model);
        }

        public IActionResult FeatureDetail(string projectid, string featureid)
        {
            PrepareDataForDisplay(projectid);
            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);

            var model = new DisplayFeatureModel(feature)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
            };

            return View(model);
        }

        public IActionResult EditFeature(string projectid, string featureid)
        {
            PrepareDataForDisplay(projectid);
            var model = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditFeature(FeatureModel model)
        {
            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(model.Project_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date < model.CreateDate.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานหลักได้ เนื่องจากวันที่เสร็จสิ้นงานหลัก ไม่สามารถน้อยกว่าวันที่สร้าง หรือไม่สามารถมากกว่าวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(model.Project_id);
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {
                    HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
                    if (isLogin.Length == 0) return RedirectToAction("Login", "Account");

                    var json = System.Text.Encoding.UTF8.GetString(isLogin);
                    var user = JsonConvert.DeserializeObject<AccountModel>(json);
                    ViewBag.User = user;

                    model.AssginByMember_id = user._id;
                }

                featureSvc.EditFeature(model);
                return RedirectToAction(nameof(FeatureDetail), new { projectid = model.Project_id, featureid = model._id });
            }
            return View(model);
        }

        public IActionResult DeleteFeature(string projectid, string featureid)
        {
            featureSvc.DeleteFeature(featureid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion Features

        #region Stories

        public IActionResult CreateStory(string projectid, string featureid)
        {
            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;

            return View(new StoryModel { Feature_id = featureid });
        }

        [HttpPost]
        public IActionResult CreateStory(string projectid, StoryModel model)
        {
            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(projectid);
                var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == model.Feature_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date > feature.ClosingDate.Date || model.ClosingDate.Date < DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานรองได้ เนื่องจากวันที่เสร็จสิ้นงานรอง ไม่สามารถน้อยกว่าวันที่ปัจจุบัน ไม่สามารถมากกว่าวันที่เสร็จสิ้นงานหลัก หรือวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = projectid;
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);
                if (!string.IsNullOrEmpty(model.BeAssignedMember_id)) model.AssginByMember_id = model.CreateByMember_id;

                storySvc.CreateStory(model);
                return RedirectToAction("Index", "Project", new { projectid = projectid });
            }
            ViewBag.ProjectId = projectid;
            return View(model);
        }

        public IActionResult StoryDetail(string projectid, string featureid, string storyid)
        {
            PrepareDataForDisplay(projectid);
            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);

            ViewBag.ProjectId = projectid;
            ViewBag.FeatureName = feature.Name;
            var model = new DisplayStoryModel(story)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
            };
            return View(model);
        }

        public IActionResult EditStory(string projectid, string featureid, string storyid)
        {
            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = projectid;
            ViewBag.Feature = featureid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;
            var model = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditStory(string projectid, StoryModel model)
        {
            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(projectid);
                var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == model.Feature_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date > feature.ClosingDate.Date || model.ClosingDate.Date < model.CreateDate.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานรองได้ เนื่องจากวันที่เสร็จสิ้นงานรอง ไม่สามารถน้อยกว่าวันที่สร้าง ไม่สามารถมากกว่าวันที่เสร็จสิ้นงานหลัก หรือวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = projectid;
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {
                    HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
                    if (isLogin.Length == 0) return RedirectToAction("Login", "Account");

                    var json = System.Text.Encoding.UTF8.GetString(isLogin);
                    var user = JsonConvert.DeserializeObject<AccountModel>(json);
                    ViewBag.User = user;

                    model.AssginByMember_id = user._id;
                }
                storySvc.EditStory(model);
                return RedirectToAction(nameof(StoryDetail), new { projectid = projectid, featureid = model.Feature_id, storyid = model._id });
            }
            ViewBag.ProjectId = projectid;
            return View(model);
        }

        public IActionResult DeleteStory(string projectid, string storyid)
        {
            storySvc.DeleteStory(storyid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion

        #region Tasks

        public IActionResult CreateTask(string projectid, string featureid, string storyid)
        {
            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            ViewBag.StoryName = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid).Name;
            return View(new TaskModel { Story_id = storyid });
        }

        [HttpPost]
        public IActionResult CreateTask(string projectid, string featureid, TaskModel model)
        {
            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == featureid);
            if (ModelState.IsValid)
            {
                var story = storySvc.GetStories(feature._id).FirstOrDefault(it => it._id == model.Story_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date > feature.ClosingDate.Date || model.ClosingDate.Date > story.ClosingDate.Date || model.ClosingDate.Date < DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานย่อยได้ เนื่องจากวันที่เสร็จสิ้นงานย่อย ไม่สามารถน้อยกว่าวันที่ปัจจุบัน ไม่สามารถมากกว่าวันที่เสร็จสิ้นงานหลัก วันที่เสร็จสิ้นงานรอง หรือวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = projectid;
                    ViewBag.FeatureId = feature._id;
                    ViewBag.FeatureName = feature.Name;
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);
                if (!string.IsNullOrEmpty(model.BeAssignedMember_id)) model.AssginByMember_id = model.CreateByMember_id;

                taskSvc.CreateTask(model);
                return RedirectToAction("Index", "Project", new { projectid = projectid });
            }
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            return View(model);
        }

        public IActionResult TaskDetail(string projectid, string featureid, string storyid, string taskid)
        {
            PrepareDataForDisplay(projectid);
            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            var task = taskSvc.GetTasks(storyid).FirstOrDefault(it => it._id == taskid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);

            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            ViewBag.StoryName = story.Name;
            var model = new DisplayTaskModel(task)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
            };
            return View(model);
        }

        public IActionResult EditTask(string projectid, string featureid, string storyid, string taskid)
        {
            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = featureid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;
            ViewBag.StoryName = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid).Name;
            var model = taskSvc.GetTasks(storyid).FirstOrDefault(it => it._id == taskid);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditTask(string projectid, string featureid, TaskModel model)
        {
            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == featureid);
            if (ModelState.IsValid)
            {
                var story = storySvc.GetStories(feature._id).FirstOrDefault(it => it._id == model.Story_id);
                if (model.ClosingDate.Date > project.ClosingDate.Date || model.ClosingDate.Date > feature.ClosingDate.Date || model.ClosingDate.Date > story.ClosingDate.Date || model.ClosingDate.Date < model.CreateDate.Date)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถสร้างงานย่อยได้ เนื่องจากวันที่เสร็จสิ้นงานย่อย ไม่สามารถน้อยกว่าวันที่สร้าง ไม่สามารถมากกว่าวันที่เสร็จสิ้นงานหลัก วันที่เสร็จสิ้นงานรอง หรือวันที่เสร็จสิ้นโปรเจคได้";
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = projectid;
                    ViewBag.FeatureId = feature._id;
                    ViewBag.FeatureName = feature.Name;
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {
                    HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
                    if (isLogin.Length == 0) return RedirectToAction("Login", "Account");

                    var json = System.Text.Encoding.UTF8.GetString(isLogin);
                    var user = JsonConvert.DeserializeObject<AccountModel>(json);
                    ViewBag.User = user;

                    model.AssginByMember_id = user._id;
                }
                taskSvc.EditTask(model);
                return RedirectToAction(nameof(TaskDetail), new { projectid = projectid, featureid = featureid, storyid = model.Story_id, taskid = model._id });
            }
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            return View(model);
        }

        public IActionResult DeleteTask(string projectid, string taskid)
        {
            taskSvc.DeleteTask(taskid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion Tasks

        /// <summary>
        /// Prepare for display work information (e.g. CreateByUser, ProjectName, ...)
        /// </summary>
        /// <param name="projectid"></param>
        private void PrepareDataForDisplay(string projectid)
        {
            var accountMemberships = new List<AccountModel>();
            var displayMemberships = new List<DisplayMembership>();

            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var projectInfo = projectSvc.GetProject(projectid);
            var allAcc = accountSvc.GetAllAccount();
            var ranks = rankSvc.GetAllRank();
            var statuses = statusSvc.GetAllStatus();

            foreach (var item in allAcc)
            {
                if (memberships.FirstOrDefault(it => it.Account_id == item._id && !it.RemoveDate.HasValue) != null) accountMemberships.Add(item);
            }

            foreach (var item in accountMemberships)
            {
                var membership = memberships.FirstOrDefault(it => it.Account_id == item._id);
                var rankName = ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id).RankName;
                var modelMembership = new DisplayMembership(membership);
                modelMembership.AccountName = $"{item.FirstName} {item.LastName}";
                modelMembership.Email = item.Email;
                modelMembership.RankName = rankName;

                displayMemberships.Add(modelMembership);
            };

            HttpContext.Session.TryGetValue("LoginData", out byte[] isLogin);
            var json = System.Text.Encoding.UTF8.GetString(isLogin);
            var user = JsonConvert.DeserializeObject<AccountModel>(json);

            ViewBag.CreateByUser = new DisplayMembership { Account_id = user._id, AccountName = $"{user.FirstName} {user.LastName}" };
            ViewBag.ProjectName = projectInfo.ProjectName;
            ViewBag.Memberships = displayMemberships;
            ViewBag.Statuses = statuses;
        }
    }
}