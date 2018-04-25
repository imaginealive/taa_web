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
        private IMembershipService membershipSvc;
        private IAccountService accountSvc;
        private IRankService rankSvc;
        public ProjectController(IProjectService projectSvc, IMembershipService membershipSvc, IAccountService accountSvc, IRankService rankSvc)
        {
            this.projectSvc = projectSvc;
            this.membershipSvc = membershipSvc;
            this.accountSvc = accountSvc;
            this.rankSvc = rankSvc;
        }

        #region Projects

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

                displayMemberships.Add(new DisplayMembership
                {
                    _id = item._id,
                    Account_id = item._id,
                    CreateDate = membership.CreateDate,
                    ProjectRank_id = membership.ProjectRank_id,
                    Project_id = membership.Project_id,
                    RemoveDate = membership.RemoveDate,
                    AccountName = item.AccountName,
                    Email = item.Email,
                    RankName = rankName
                });
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

                displayMemberships.Add(new DisplayMembership
                {
                    _id = item._id,
                    Account_id = item._id,
                    CreateDate = membership.CreateDate,
                    ProjectRank_id = membership.ProjectRank_id,
                    Project_id = membership.Project_id,
                    RemoveDate = membership.RemoveDate,
                    AccountName = item.AccountName,
                    Email = item.Email,
                    RankName = rankName
                });
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
            var memberships = membershipSvc.GetAllProjectMember(projectid);
            var membership = memberships.FirstOrDefault(it => it.Account_id == accountid);
            membership.ProjectRank_id = rankid;

            membershipSvc.EditMember(membership);
            return RedirectToAction(nameof(AllMemberships), new { projectid = projectid });
        }

        #endregion Memberships
    }
}