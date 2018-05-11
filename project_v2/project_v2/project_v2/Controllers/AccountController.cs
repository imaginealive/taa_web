using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
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
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(model);
                var newUser = JsonConvert.DeserializeObject<AccountModel>(json);
                svc.CreateAccount(newUser);
                var user = svc.Login(model.AccountName, model.Password);


                // create claims
                List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, JsonConvert.SerializeObject(user)),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

                // create identity
                ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

                // create principal
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                // sign-in
                await HttpContext.SignInAsync(scheme: "FiverSecurityScheme", principal: principal);
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
        public async Task<IActionResult> Login(AccountModel model)
        {
            var user = svc.Login(model.AccountName, model.Password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "ไม่พบผู้ใช้งาน กรุณาตรวจสอบชื่อผู้ใช้ และรหัสผ่านอีกครั้ง";
                return View(model);
            }

            // create claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, JsonConvert.SerializeObject(user)),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            // create identity
            ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

            // create principal
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // sign-in
            await HttpContext.SignInAsync(scheme: "FiverSecurityScheme", principal: principal);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult EditProfile()
        {

            var userString = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var userdata = JsonConvert.DeserializeObject<AccountModel>(userString);
            return View(userdata);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(AccountModel body)
        {
            if (ModelState.IsValid)
            {
                svc.EditAccount(body);
                // create claims
                List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, JsonConvert.SerializeObject(body)),
                new Claim(ClaimTypes.Role, body.IsAdmin ? "Admin" : "User")
            };

                // create identity
                ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

                // create principal
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                // sign-in
                await HttpContext.SignInAsync(scheme: "FiverSecurityScheme", principal: principal);
                return RedirectToAction("Index", "Home");
            }
            return View(body);
        }
    }
}