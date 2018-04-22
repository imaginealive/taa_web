using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private IStatusService statussvc;

        public AdminController(IRankService _ranksvc,
            IStatusService _statussvc,
            IAccountService _accountsvc)
        {
            this.accountsvc = _accountsvc;
            this.ranksvc = _ranksvc;
            this.statussvc = _statussvc;

        }

        public IActionResult AccountManage()
        {
            var model = accountsvc.GetAllAccount();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditAccount(string id)
        {
            var model = accountsvc.GetAllAccount().FirstOrDefault(it => it._id == id);
            if (model == null) return RedirectToAction(nameof(AccountManage));
            return View(model);
        }

        [HttpPost]
        public IActionResult EditAccount(AccountModel body, string id)
        {
            if (ModelState.IsValid)
            {
                accountsvc.EditAccount(body);
                return RedirectToAction(nameof(AccountManage));
            }
            return View(body);
        }

        public IActionResult ProjectSystem()
        {
            var model = new ProjectSystemViewModel
            {
                ranks = ranksvc.GetAllRank(),
                status = statussvc.GetAllStatus()
            };
            return View(model);
        }
    }
}