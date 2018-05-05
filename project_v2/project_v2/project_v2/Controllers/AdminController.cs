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
    [AdminAction]
    public class AdminController : Controller
    {
        private IAccountService accountsvc;
        private IRankService ranksvc;
        IServiceConfigurations serviceConfig;
        private IStatusService statussvc;

        public AdminController(IRankService _ranksvc,
            IStatusService _statussvc,
            IAccountService _accountsvc,
            IServiceConfigurations serviceConfig)
        {
            this.accountsvc = _accountsvc;
            this.ranksvc = _ranksvc;
            this.statussvc = _statussvc;
            this.serviceConfig = serviceConfig;
        }

        public IActionResult AccountManage()
        {
            var model = accountsvc.GetAllAccount();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditAccount(string id)
        {
            var user = accountsvc.GetAllAccount().FirstOrDefault(it => it._id == id);
            if (user == null) return RedirectToAction(nameof(AccountManage));
            var json = JsonConvert.SerializeObject(user);
            var model = JsonConvert.DeserializeObject<RegisterModel>(json);
            model.IsSuspend = model.SuspendDate.HasValue;
            return View(model);
        }

        [HttpPost]
        public IActionResult EditAccount(RegisterModel body, string id)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(body);
                var user = JsonConvert.DeserializeObject<AccountModel>(json);
                if (body.IsSuspend)
                    user.SuspendDate = DateTime.Now;
                else
                    user.SuspendDate = null;

                accountsvc.EditAccount(user);
                return RedirectToAction(nameof(AccountManage));
            }
            return View(body);
        }

        public IActionResult ProjectSystem()
        {
            ViewBag.GuestRankId = serviceConfig.GuestRankId;
            ViewBag.MasterRankId = serviceConfig.MasterRankId;
            ViewBag.NewStatusId = serviceConfig.StatusNewId;
            var model = new ProjectSystemViewModel
            {
                ranks = ranksvc.GetAllRank(),
                status = statussvc.GetAllStatus()
            };
            return View(model);
        }

        public IActionResult CreateRank()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateRank(ProjectRankModel model)
        {
            if (ModelState.IsValid)
            {
                ranksvc.AddRank(model);
                return RedirectToAction(nameof(ProjectSystem));
            }
            return View(model);
        }

        public IActionResult EditRank(string rankid)
        {
            var rankList = ranksvc.GetAllRank();
            var result = rankList.FirstOrDefault(it => it._id == rankid);
            return View(result);
        }

        [HttpPost]
        public IActionResult EditRank(ProjectRankModel model)
        {
            if (ModelState.IsValid)
            {
                ranksvc.EditRank(model);
                return RedirectToAction(nameof(ProjectSystem));
            }
            return View(model);
        }

        public IActionResult DeleteRank(string rankid)
        {
            ranksvc.DeleteRank(rankid);
            return RedirectToAction(nameof(ProjectSystem));
        }

        public IActionResult CreateStatus()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateStatus(StatusModel model)
        {
            if (ModelState.IsValid)
            {
                statussvc.AddStatus(model);
                return RedirectToAction(nameof(ProjectSystem));
            }
            return View(model);
        }

        public IActionResult EditStatus(string statusid)
        {
            var statusList = statussvc.GetAllStatus();
            var result = statusList.FirstOrDefault(it => it._id == statusid);
            return View(result);
        }

        [HttpPost]
        public IActionResult EditStatus(StatusModel model)
        {
            if (ModelState.IsValid)
            {
                statussvc.EditStatus(model);
                return RedirectToAction(nameof(ProjectSystem));
            }
            return View(model);
        }

        public IActionResult DeleteStatus(string statusid)
        {
            statussvc.DeleteStatus(statusid);
            return RedirectToAction(nameof(ProjectSystem));
        }
    }
}