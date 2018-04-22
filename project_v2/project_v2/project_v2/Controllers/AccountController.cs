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
                var json = JsonConvert.SerializeObject(model);
                var newUser = JsonConvert.DeserializeObject<AccountModel>(json);
                svc.CreateAccount(newUser);
                var user = svc.Login(model.AccountName, model.Password);
                HttpContext.Session.SetString("LoginData", JsonConvert.SerializeObject(user));
                return RedirectToAction("Index", "Home");
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
            if (user == null)
            {
                ViewBag.ErrorMessage = "ไม่พบผู้ใช้งาน กรุณาตรวจสอบชื่อผู้ใช้ และรหัสผ่านอีกครั้ง";
                return View(model);
            }

            HttpContext.Session.SetString("LoginData", JsonConvert.SerializeObject(user));
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("LoginData", string.Empty);
            return RedirectToAction("Index", "Home");
        }
    }
}