using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using taaproject.Models;
using taaproject.Services;
using static taaproject.Services.ProjectService;

namespace taaproject.Controllers
{
    public class MembershipController : Controller
    {
        private readonly MembershopServices _svc;

        public MembershipController(MembershopServices svc)
        {
            _svc = svc;
        }

        public async Task<IActionResult> Index(string projectid, string username)
        {
            var isUsernameValid = !string.IsNullOrEmpty(username);
            if (isUsernameValid)
            {
                var result = await _svc.InviteMembership(projectid, username);
                if (!result) ViewBag.ErrorMessage = "ไม่สามารถเพิ่มสมาชิกคนดังกล่าวเข้าระบบได้";
            }
            var model = await _svc.GetMemberships(projectid);
            ViewBag.ProjectId = projectid;
            return View(model);
        }

        //public IActionResult Detail()
        //{
        //    return View();
        //}

        //public IActionResult Add()
        //{
        //    return View();
        //}

        public async Task<IActionResult> Edit(string projectid, string username)
        {
            var model = await _svc.GetMembership(projectid, username);
            ViewBag.ProjectId = projectid;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string projectid, MembershipModel model)
        {
            var result = await _svc.ChangeMembershipInformation(model);
            if (!result) ViewBag.ErrorMessage = "ไม่สามารถเปลี่ยนระดับผู้ใช้งานได้";
            return RedirectToAction(nameof(Index), new { projectid = projectid });
        }

        //public IActionResult Delete()
        //{
        //    return View();
        //}
    }
}
