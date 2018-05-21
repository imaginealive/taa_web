using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class AccountController : Controller
    {
        public IAccountService svc;
        public IDistributedCache cache;

        public AccountController(IAccountService _svc, IDistributedCache _cache)
        {
            cache = _cache;
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
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(model);
                var newUser = JsonConvert.DeserializeObject<AccountModel>(json);
                svc.CreateAccount(newUser);
                var user = svc.Login(model.AccountName, model.Password);

                cache.SetString("user", JsonConvert.SerializeObject(user));
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            return View();
        }

        [HttpPost]
        public IActionResult Login(AccountModel model)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));

            var user = svc.Login(model.AccountName, model.Password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "ไม่พบผู้ใช้งาน กรุณาตรวจสอบชื่อผู้ใช้ และรหัสผ่านอีกครั้ง";
                return View(model);
            }
            else if (user.SuspendDate.HasValue)
            {
                ViewBag.ErrorMessage = "ไม่สามารถเข้าสู่ระบบได้ เนื่องจากบัญชีนี้ถูกระงับใช้งาน กรุณาติดต่อผู้ดูแลระบบ";
                return View(model);
            }
            cache.SetString("user", JsonConvert.SerializeObject(user));
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            cache.SetString("user", string.Empty);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult EditProfile()
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            var userdata = ViewBag.User;
            return View(userdata);
        }

        [HttpPost]
        public IActionResult EditProfile(AccountModel body)
        {
            ViewBag.IsLogin = !string.IsNullOrEmpty(cache.GetString("user"));
            if (ViewBag.IsLogin) ViewBag.User = JsonConvert.DeserializeObject<AccountModel>(cache.GetString("user"));
            else return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                svc.EditAccount(body);

                cache.SetString("user", JsonConvert.SerializeObject(body));
                return RedirectToAction("Index", "Home");
            }
            return View(body);
        }
    }
}