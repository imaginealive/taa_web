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
        private IStatusService statusSvc;
        public ProjectController(IProjectService projectSvc,
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

        #region Projects

        public IActionResult Index(string projectid)
        {
            // TODO: Get Works (Stories and Tasks)
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
            foreach (var item in features)
            {
                var createByAccount = allAcc.FirstOrDefault(it => it._id == item.CreateByMember_id);
                var assginByAccount = allAcc.FirstOrDefault(it => it._id == item.AssginByMember_id);
                var beassginByAccount = allAcc.FirstOrDefault(it => it._id == item.BeAssignedMember_id);
                var status = statusSvc.GetAllStatus().FirstOrDefault(it => it._id == item.StatusName);

                var model = new DisplayFeatureModel(item)
                {
                    CreateByMemberName = createByAccount != null ? $"{createByAccount.FirstName} {createByAccount.LastName}" : string.Empty,
                    AssginByMemberName = assginByAccount != null ? $"{assginByAccount.FirstName} {assginByAccount.LastName}" : string.Empty,
                    BeAssignedMemberName = beassginByAccount != null ? $"{beassginByAccount.FirstName} {beassginByAccount.LastName}" : string.Empty,
                    Status = status != null ? status.StatusName : string.Empty
                };
                displayFeatures.Add(model);
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

            ViewBag.AllRanks = ranks;
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
            var ranks = rankSvc.GetAllRank();
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