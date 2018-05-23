﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
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
        public IDistributedCache cache;
        public IAssignmentService assignmentSvc;

        public WorkController(
            IProjectService projectSvc,
            IFeatureService featureSvc,
            IStoryService storySvc,
            ITaskService taskSvc,
            IMembershipService membershipSvc,
            IAccountService accountSvc,
            IRankService rankSvc,
            IStatusService statusSvc,
            IDistributedCache _cache,
            IAssignmentService assignmentSvc)
        {
            cache = _cache;
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
            this.storySvc = storySvc;
            this.taskSvc = taskSvc;
            this.membershipSvc = membershipSvc;
            this.accountSvc = accountSvc;
            this.rankSvc = rankSvc;
            this.statusSvc = statusSvc;
            this.assignmentSvc = assignmentSvc;
        }

        #region Features

        public IActionResult CreateFeature(string projectid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            PrepareDataForDisplay(projectid);
            return View(new FeatureModel { Project_id = projectid });
        }

        [HttpPost]
        public IActionResult CreateFeature(FeatureModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(model.Project_id);
                var isValid = ValidateClosingDate(true, project, model);
                if (!isValid)
                {
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
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == feature.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == feature.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == feature.BeAssignedMember_id);

            var assignmentHistories = assignmentSvc.GetAssignments(feature._id, WorkType.Feature);
            var displayAssignmentHistories = new List<DisplayAssignmentModel>();
            foreach (var item in assignmentHistories)
            {
                var assignedAccountHistory = allAcc.FirstOrDefault(it => it._id == item.Member_id);
                var display = new DisplayAssignmentModel(item) { MemberName = assignedAccountHistory != null ? $"{assignedAccountHistory.FirstName} {assignedAccountHistory.LastName}" : string.Empty };
                displayAssignmentHistories.Add(display);
            }

            var model = new DisplayFeatureModel(feature)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
                assignmentHistories = displayAssignmentHistories.OrderByDescending(it => it.AssignDate)
            };

            PrepareDataForDisplay(projectid, feature);
            return View(model);
        }

        public IActionResult EditFeature(string projectid, string featureid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var model = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            PrepareDataForDisplay(projectid, model);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditFeature(FeatureModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var project = projectSvc.GetProject(model.Project_id);
                var isValid = ValidateClosingDate(false, project, model);
                if (!isValid)
                {
                    PrepareDataForDisplay(model.Project_id, model);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {

                    var memberships = membershipSvc.GetAllProjectMember(model.Project_id);
                    var ranks = rankSvc.GetAllRank();
                    var member = ViewBag.User != null ? memberships.FirstOrDefault(it => it.Account_id == ViewBag.User._id && !it.RemoveDate.HasValue) : null;
                    var userRank = (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id));
                    var canAssign = userRank.CanAssign || member.CanAssign;
                    if (canAssign) model.AssginByMember_id = ViewBag.User._id;
                }
                else model.AssginByMember_id = string.Empty;
                model.ClosingDate = model.ClosingDate.AddDays(1);

                featureSvc.EditFeature(model);
                return RedirectToAction(nameof(FeatureDetail), new { projectid = model.Project_id, featureid = model._id });
            }
            PrepareDataForDisplay(model.Project_id, model);
            return View(model);
        }

        public IActionResult DeleteFeature(string projectid, string featureid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            featureSvc.DeleteFeature(featureid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion Features

        #region Stories

        public IActionResult CreateStory(string projectid, string featureid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = projectid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;

            return View(new StoryModel { Feature_id = featureid });
        }

        [HttpPost]
        public IActionResult CreateStory(string projectid, StoryModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == model.Feature_id);
            if (ModelState.IsValid)
            {
                var isValid = ValidateClosingDate(true, project, feature, model);
                if (!isValid)
                {
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = project._id;
                    ViewBag.FeatureName = feature.Name;
                    return View(model);
                }
                model.ClosingDate = model.ClosingDate.AddDays(1);
                if (!string.IsNullOrEmpty(model.BeAssignedMember_id)) model.AssginByMember_id = model.CreateByMember_id;

                storySvc.CreateStory(model);
                return RedirectToAction("Index", "Project", new { projectid = projectid });
            }
            PrepareDataForDisplay(projectid);
            ViewBag.ProjectId = project._id;
            ViewBag.FeatureName = feature.Name;
            return View(model);
        }

        public IActionResult StoryDetail(string projectid, string featureid, string storyid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == story.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == story.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == story.BeAssignedMember_id);

            var assignmentHistories = assignmentSvc.GetAssignments(feature._id, WorkType.Story);
            var displayAssignmentHistories = new List<DisplayAssignmentModel>();
            foreach (var item in assignmentHistories)
            {
                var assignedAccountHistory = allAcc.FirstOrDefault(it => it._id == item.Member_id);
                var display = new DisplayAssignmentModel(item) { MemberName = assignedAccountHistory != null ? $"{assignedAccountHistory.FirstName} {assignedAccountHistory.LastName}" : string.Empty };
                displayAssignmentHistories.Add(display);
            }

            ViewBag.ProjectId = projectid;
            ViewBag.FeatureName = feature.Name;
            var model = new DisplayStoryModel(story)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
                assignmentHistories = displayAssignmentHistories.OrderByDescending(it => it.AssignDate)

            };
            PrepareDataForDisplay(projectid, story);
            return View(model);
        }

        public IActionResult EditStory(string projectid, string featureid, string storyid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            ViewBag.ProjectId = projectid;
            ViewBag.Feature = featureid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;
            var model = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            PrepareDataForDisplay(projectid, model);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditStory(string projectid, StoryModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == model.Feature_id);
            if (ModelState.IsValid)
            {
                var isValid = ValidateClosingDate(false, project, feature, model);
                if (!isValid)
                {
                    PrepareDataForDisplay(projectid, model);
                    ViewBag.ProjectId = project._id;
                    ViewBag.FeatureName = feature.Name;
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {

                    var memberships = membershipSvc.GetAllProjectMember(projectid);
                    var ranks = rankSvc.GetAllRank();
                    var member = ViewBag.User != null ? memberships.FirstOrDefault(it => it.Account_id == ViewBag.User._id && !it.RemoveDate.HasValue) : null;
                    var userRank = (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id));
                    var canAssign = userRank.CanAssign || member.CanAssign;
                    if (canAssign) model.AssginByMember_id = ViewBag.User._id;
                }
                else model.AssginByMember_id = string.Empty;
                model.ClosingDate = model.ClosingDate.AddDays(1);

                storySvc.EditStory(model);
                return RedirectToAction(nameof(StoryDetail), new { projectid = projectid, featureid = model.Feature_id, storyid = model._id });
            }
            PrepareDataForDisplay(projectid, model);
            ViewBag.ProjectId = project._id;
            ViewBag.FeatureName = feature.Name;
            return View(model);
        }

        public IActionResult DeleteStory(string projectid, string storyid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            storySvc.DeleteStory(storyid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion

        #region Tasks

        public IActionResult CreateTask(string projectid, string featureid, string storyid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

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
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(feature._id).FirstOrDefault(it => it._id == model.Story_id);
            if (ModelState.IsValid)
            {
                var isValid = ValidateClosingDate(true, project, feature, story, model);
                if (!isValid)
                {
                    PrepareDataForDisplay(projectid);
                    ViewBag.ProjectId = project._id;
                    ViewBag.FeatureId = feature._id;
                    ViewBag.FeatureName = feature.Name;
                    ViewBag.StoryName = story.Name;
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
            ViewBag.StoryName = story.Name;
            return View(model);
        }

        public IActionResult TaskDetail(string projectid, string featureid, string storyid, string taskid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var feature = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid);
            var task = taskSvc.GetTasks(storyid).FirstOrDefault(it => it._id == taskid);
            var allAcc = accountSvc.GetAllAccount();

            var createByAccount = allAcc.FirstOrDefault(it => it._id == task.CreateByMember_id);
            var assginByAccount = allAcc.FirstOrDefault(it => it._id == task.AssginByMember_id);
            var beassginByAccount = allAcc.FirstOrDefault(it => it._id == task.BeAssignedMember_id);

            var assignmentHistories = assignmentSvc.GetAssignments(feature._id, WorkType.Task);
            var displayAssignmentHistories = new List<DisplayAssignmentModel>();
            foreach (var item in assignmentHistories)
            {
                var assignedAccountHistory = allAcc.FirstOrDefault(it => it._id == item.Member_id);
                var display = new DisplayAssignmentModel(item) { MemberName = assignedAccountHistory != null ? $"{assignedAccountHistory.FirstName} {assignedAccountHistory.LastName}" : string.Empty };
                displayAssignmentHistories.Add(display);
            }

            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            ViewBag.StoryName = story.Name;
            var model = new DisplayTaskModel(task)
            {
                CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
                assignmentHistories = displayAssignmentHistories.OrderByDescending(it => it.AssignDate)
            };
            PrepareDataForDisplay(projectid, task);
            return View(model);
        }

        public IActionResult EditTask(string projectid, string featureid, string storyid, string taskid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            ViewBag.ProjectId = projectid;
            ViewBag.FeatureId = featureid;
            ViewBag.FeatureName = featureSvc.GetFeatures(projectid).FirstOrDefault(it => it._id == featureid).Name;
            ViewBag.StoryName = storySvc.GetStories(featureid).FirstOrDefault(it => it._id == storyid).Name;
            var model = taskSvc.GetTasks(storyid).FirstOrDefault(it => it._id == taskid);
            PrepareDataForDisplay(projectid, model);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditTask(string projectid, string featureid, TaskModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var project = projectSvc.GetProject(projectid);
            var feature = featureSvc.GetFeatures(project._id).FirstOrDefault(it => it._id == featureid);
            var story = storySvc.GetStories(feature._id).FirstOrDefault(it => it._id == model.Story_id);
            if (ModelState.IsValid)
            {
                var isValid = ValidateClosingDate(false, project, feature, story, model);
                if (!isValid)
                {
                    PrepareDataForDisplay(projectid, model);
                    ViewBag.ProjectId = project._id;
                    ViewBag.FeatureId = feature._id;
                    ViewBag.FeatureName = feature.Name;
                    ViewBag.StoryName = story.Name;
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.BeAssignedMember_id))
                {
                    var memberships = membershipSvc.GetAllProjectMember(projectid);
                    var ranks = rankSvc.GetAllRank();
                    var member = ViewBag.User != null ? memberships.FirstOrDefault(it => it.Account_id == ViewBag.User._id && !it.RemoveDate.HasValue) : null;
                    var userRank = (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id));
                    var canAssign = userRank.CanAssign || member.CanAssign;
                    if (canAssign) model.AssginByMember_id = ViewBag.User._id;
                }
                else model.AssginByMember_id = string.Empty;
                model.ClosingDate = model.ClosingDate.AddDays(1);

                taskSvc.EditTask(model);
                return RedirectToAction(nameof(TaskDetail), new { projectid = projectid, featureid = featureid, storyid = model.Story_id, taskid = model._id });
            }
            PrepareDataForDisplay(projectid, model);
            ViewBag.ProjectId = project._id;
            ViewBag.FeatureId = feature._id;
            ViewBag.FeatureName = feature.Name;
            ViewBag.StoryName = story.Name;
            return View(model);
        }

        public IActionResult DeleteTask(string projectid, string taskid)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            taskSvc.DeleteTask(taskid);
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { projectid = projectid });
        }

        #endregion Tasks

        /// <summary>
        /// Prepare for display work information (e.g. CreateByUser, ProjectName, ...)
        /// </summary>
        /// <param name="projectid"></param>
        private void PrepareDataForDisplay(string projectid, WorkModel work = null)
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
                var CanBeAssign = membership != null ? ranks.FirstOrDefault(it => it._id == membership.ProjectRank_id).BeAssigned || membership.BeAssigned : false;
                if (CanBeAssign)
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
                    var modelMembership = new DisplayMembership(membership);
                    modelMembership.AccountName = $"{item.FirstName} {item.LastName}";
                    modelMembership.Email = item.Email;
                    modelMembership.RankName = rankName;
                    modelMembership.AllWorkHasBeenAssigned = allWorkHasBeenAssigned;

                    displayMemberships.Add(modelMembership);
                }
            };

            var currentUser = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));

            // Check current user permission
            var member = currentUser != null ? memberships.FirstOrDefault(it => it.Account_id == currentUser._id && !it.RemoveDate.HasValue) : null;

            if (work != null)
            {
                var createByAccount = allAcc.FirstOrDefault(it => it._id == work.CreateByMember_id);
                ViewBag.CreateByUser = new DisplayMembership { Account_id = createByAccount._id, AccountName = $"{createByAccount.FirstName} {createByAccount.LastName}" };

                var assignedByAccount = allAcc.FirstOrDefault(it => it._id == work.BeAssignedMember_id);
                ViewBag.BeAssignedMemberName = assignedByAccount != null ? $"{assignedByAccount.FirstName} {assignedByAccount.LastName}" : string.Empty;

                ViewBag.CanEditOrUpdateThisWork = member != null ?
                    (work.BeAssignedMember_id == currentUser._id && (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).BeAssigned || member.BeAssigned)) ||
                    (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanEditAllWork) || member.CanEditAllWork : false;
                ViewBag.CanEditWorkInformation = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanEditAllWork) || member.CanEditAllWork : false;
                ViewBag.Statuses = !string.IsNullOrEmpty(work.BeAssignedMember_id) ? statuses : statuses.Where(it => !it.IsWorkDone);
            }
            else
            {
                ViewBag.CreateByUser = new DisplayMembership { Account_id = currentUser._id, AccountName = $"{currentUser.FirstName} {currentUser.LastName}" };
                ViewBag.Statuses = statuses.Where(it => !it.IsWorkDone);
            }

            ViewBag.CanAssign = member != null ? (ranks.FirstOrDefault(it => it._id == member.ProjectRank_id).CanAssign) || member.CanAssign : false;
            ViewBag.ProjectName = projectInfo.ProjectName;
            ViewBag.Memberships = displayMemberships;
        }

        /// <summary>
        /// Validation ClosingDate each WorkModel
        /// </summary>
        /// <param name="isCreateWork">Validate in Create or Edit process</param>
        /// <param name="project">Project to validate</param>
        /// <param name="feature">Feature to validate</param>
        /// <param name="story">Story to validate</param>
        /// <param name="task">Task to validate</param>
        /// <returns>Result of validation</returns>
        private bool ValidateClosingDate(bool isCreateWork, ProjectModel project, FeatureModel feature, StoryModel story = null, TaskModel task = null)
        {
            var validateProject = project != null;
            if (!validateProject) return false;

            var isVerifyFeature = feature != null && story == null && task == null;
            var isVerifyStory = feature != null && story != null && task == null;
            var isVerifyTask = feature != null && story != null && task != null;

            var errorTitleCategory = isCreateWork ? "สร้าง" : "แก้ไข";
            var workCategory = isVerifyFeature ? "งานหลัก" : isVerifyStory ? "งานรอง" : "งานย่อย";

            var value = isVerifyFeature ? feature as WorkModel : isVerifyStory ? story as WorkModel : task as WorkModel;

            var isValidClosingDate = isCreateWork ? value.ClosingDate.Date >= DateTime.Now.Date : value.ClosingDate.Date >= value.CreateDate.Date;
            if (!isValidClosingDate)
            {
                var postErrorMessage = isCreateWork ? "วันที่ปัจจุบัน" : "วันที่สร้าง";
                ViewBag.ErrorTitle = $"ไม่สามารถ{errorTitleCategory}{workCategory}ได้";
                ViewBag.ErrorMessage = $"เนื่องจาก: วันที่คาดการณ์งานต้องเสร็จสิ้นของ{workCategory} น้อยกว่า{postErrorMessage}";
                return false;
            }

            var isValidClosingDateWithProject = value.ClosingDate.Date <= project.ClosingDate.Date;
            if (!isValidClosingDateWithProject)
            {
                ViewBag.ErrorTitle = $"ไม่สามารถ{errorTitleCategory}{workCategory}ได้";
                ViewBag.ErrorMessage = $"เนื่องจาก: วันที่คาดการณ์งานต้องเสร็จสิ้นของ{workCategory} มากกว่าวันที่เสร็จสิ้นของโปรเจค";
                return false;
            }

            if (isVerifyStory)
            {
                var isValidClosingDateWithFeature = value.ClosingDate.Date <= feature.ClosingDate.Date;
                if (!isValidClosingDateWithFeature)
                {
                    ViewBag.ErrorTitle = $"ไม่สามารถ{errorTitleCategory}{workCategory}ได้";
                    ViewBag.ErrorMessage = $"เนื่องจาก: วันที่คาดการณ์งานต้องเสร็จสิ้นของ{workCategory} มากกว่าวันที่เสร็จสิ้นของงานหลัก";
                    return false;
                }
            }

            if (isVerifyTask)
            {
                var isValidClosingDateWithStory = value.ClosingDate.Date <= story.ClosingDate.Date;
                if (!isValidClosingDateWithStory)
                {
                    ViewBag.ErrorTitle = $"ไม่สามารถ{errorTitleCategory}{workCategory}ได้";
                    ViewBag.ErrorMessage = $"เนื่องจาก: วันที่คาดการณ์งานต้องเสร็จสิ้นของ{workCategory} มากกว่าวันที่เสร็จสิ้นของงานรอง";
                    return false;
                }
            }
            return true;
        }
    }
}