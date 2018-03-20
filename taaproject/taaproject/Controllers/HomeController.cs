using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using taaproject.Models;
using Microsoft.AspNetCore.Authorization;
using static taaproject.Services.ProjectService;
using taaproject.Models.HomeViewModels;
using taaproject.Services;

namespace taaproject.Controllers
{
    using Microsoft.AspNetCore.Identity.MongoDB;
    using static taaproject.Services.WorkService;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ProjectService _svc;
        private readonly MembershopServices _membershipSVC;
        private readonly WorkService _WorkSVC;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ProjectService svc,
            MembershopServices membershipsvc,
            WorkService worksvc)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _svc = svc;
            _membershipSVC = membershipsvc;
            _WorkSVC = worksvc;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _svc.GetAllAllowProjectAsync(User);
            var model = projects.ToList();
            return View(model);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var model = await _svc.GetAllowProjectAsync(id, User);
            var qry = await _WorkSVC.GetAllAllowWorkAsync(id, User);
            model.Work = qry.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ProjectModel { StartDate = DateTime.Now };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectModel model)
        {
            var now = DateTime.UtcNow;
            if (model.FinishDate.HasValue)
                if (model.FinishDate < model.StartDate || model.FinishDate < now)
                {
                    ModelState.AddModelError(nameof(model.FinishDate), "วันหมดอายุต้องมากกว่าวันเริ่มใช้งาน");
                }

            if (ModelState.IsValid)
            {
                try
                {
                    await _svc.CreateProjectAsync(model, User);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var project = await _svc.GetAllowProjectAsync(id, User);
            var model = new ProjectModel
            {
                _id = project._id,
                ProjectName = project.ProjectName,
                Description = project.Description,
                StartDate = project.StartDate,
                FinishDate = project.FinishDate,
            };
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(string id, ProjectModel model)
        {
            var now = DateTime.UtcNow;
            if (model.FinishDate.HasValue)
                if (model.FinishDate < model.StartDate || model.FinishDate < now)
                {
                    ModelState.AddModelError(nameof(model.FinishDate), "วันหมดอายุต้องมากกว่าวันเริ่มใช้งาน");
                }
            if (ModelState.IsValid)
            {
                try
                {
                    await _svc.UpdateProjectAsync(model, User);
                    return RedirectToAction(nameof(Detail), new { id });
                }
                catch (Exception)
                {
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _svc.DeleteProjectAsync(id, User);
            return RedirectToAction(nameof(Index));
        }

        #region Features
        
        public async Task<IActionResult> AddNewFeature(string projectid)
        {
            ViewBag.Username = _userManager.GetUserName(User);
            var qry = await _membershipSVC.GetMemberships(projectid);
            ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
            return View(new FeatureModel { Project_id = projectid });
        }

        [HttpPost]
        public async Task<IActionResult> AddNewFeature(FeatureModel request)
        {
            if (ModelState.IsValid)
            {
                var reulst = await _WorkSVC.CreateFeature(request);
                if (!reulst)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถเพิ่มงานหลักได้ กรุณาตรวจสอบข้อมูล";
                    var qry = await _membershipSVC.GetMemberships(request.Project_id);
                    ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                    return View(request);
                }
                return RedirectToAction(nameof(Detail), new { id = request.Project_id });
            }
            else
            {
                ViewBag.ErrorMessage = "ไม่สามารถเพิ่มงานหลักได้ กรุณาตรวจสอบข้อมูล";
                var qry = await _membershipSVC.GetMemberships(request.Project_id);
                ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                return View(request);
            }
        }

        public async Task<IActionResult> EditFeature(string featureid)
        {
            var model = await _WorkSVC.GetFeature(featureid);
            var qry = await _membershipSVC.GetMemberships(model.Project_id);
            ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditFeature(FeatureModel request)
        {
            if (ModelState.IsValid)
            {
                var result = await _WorkSVC.UpdateFeature(request, User);
                if (!result)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถแก้ไขข้อมูลงานหลักได้ กรุณาตรวจสอบข้อมูล";
                    var qry = await _membershipSVC.GetMemberships(request.Project_id);
                    ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                    return View(request);
                }
                return RedirectToAction(nameof(Detail), new { id = request.Project_id });
            }
            else
            {
                ViewBag.ErrorMessage = "ไม่สามารถแก้ไขข้อมูลงานหลักได้ กรุณาตรวจสอบข้อมูล";
                var qry = await _membershipSVC.GetMemberships(request.Project_id);
                ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                return View(request);
            }
        }

        public async Task<IActionResult> RemoveFeature(string projectid, string featureid)
        {
            await _WorkSVC.RemoveFeature(featureid);
            return RedirectToAction(nameof(Detail), new { id = projectid });
        }

        #endregion Features

        #region Stories

        public async Task<IActionResult> AddNewStory(string projectid, string featureid)
        {
            ViewBag.Username = _userManager.GetUserName(User);
            var qry = await _membershipSVC.GetMemberships(projectid);
            ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
            return View(new StoryModel { Project_id = projectid, Feature_id = featureid });
        }
        
        [HttpPost]
        public async Task<IActionResult> AddNewStory(StoryModel request)
        {
            if (ModelState.IsValid)
            {
                var reulst = await _WorkSVC.CreateStory(request);
                if (!reulst)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถเพิ่มงานรองได้ กรุณาตรวจสอบข้อมูล";
                    var qry = await _membershipSVC.GetMemberships(request.Project_id);
                    ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                    return View(request);
                }
                return RedirectToAction(nameof(Detail), new { id = request.Project_id });
            }
            else
            {
                ViewBag.ErrorMessage = "ไม่สามารถเพิ่มงานรองได้ กรุณาตรวจสอบข้อมูล";
                var qry = await _membershipSVC.GetMemberships(request.Project_id);
                ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                return View(request);
            }
        }
        
        public async Task<IActionResult> EditStory(string storyid)
        {
            var model = await _WorkSVC.GetStory(storyid);
            var qry = await _membershipSVC.GetMemberships(model.Project_id);
            ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> EditStory(StoryModel request)
        {
            if (ModelState.IsValid)
            {
                var result = await _WorkSVC.UpdateStory(request, User);
                if (!result)
                {
                    ViewBag.ErrorMessage = "ไม่สามารถแก้ไขข้อมูลงานหลักได้ กรุณาตรวจสอบข้อมูล";
                    var qry = await _membershipSVC.GetMemberships(request.Project_id);
                    ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                    return View(request);
                }
                return RedirectToAction(nameof(Detail), new { id = request.Project_id });
            }
            else
            {
                ViewBag.ErrorMessage = "ไม่สามารถแก้ไขข้อมูลงานหลักได้ กรุณาตรวจสอบข้อมูล";
                var qry = await _membershipSVC.GetMemberships(request.Project_id);
                ViewBag.AssignmentList = qry.Select(it => it.MemberUserName);
                return View(request);
            }
        }
        
        public async Task<IActionResult> RemoveStory(string projectid, string storyid)
        {
            await _WorkSVC.RemoveStory(storyid);
            return RedirectToAction(nameof(Detail), new { id = projectid });
        }

        #endregion Stories
        
        public IActionResult AddNewTask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewTask(string Name, string Description)
        {
            var isValidData = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Name or Description can not be empty";
                return View();
            }

            return View(nameof(Detail));
        }
        
        public IActionResult EditTask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EditTask(string Description, string AssignmentMember, string inlineRadioOptions)
        {
            var isValidData = !string.IsNullOrEmpty(Description) && !string.IsNullOrEmpty(AssignmentMember) && !string.IsNullOrEmpty(inlineRadioOptions);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Description or AssignmentMember or InlineRadioOptions can not be empty";
                return View();
            }

            return View(nameof(Detail));
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
