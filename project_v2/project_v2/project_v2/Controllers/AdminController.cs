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
            var model = new ProjectSystemViewModel
            {
                ranks = ranksvc.GetAllRank(),
                status = statussvc.GetAllStatus()
            };
            return View(model);
        }
    }
}