using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class AccountController : Controller
    {
        public IAccountService svc;

        public AccountController(IAccountService _svc)
        {
            svc = _svc;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                svc.CreateAccount(model as AccountModel);
                var user = svc.Login(model.AccountName, model.Password);
                HttpContext.Session.SetString("LoginData", JsonConvert.SerializeObject(user));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AccountModel model)
        {
            var user = svc.Login(model.AccountName, model.Password);
            if (user == null) return View(model);

            HttpContext.Session.SetString("LoginData", JsonConvert.SerializeObject(user));
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("LoginData", null);
            return RedirectToAction("Index", "Home");
        }
    }
}