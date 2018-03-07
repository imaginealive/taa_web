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

namespace taaproject.Controllers
{
    using Microsoft.AspNetCore.Identity.MongoDB;
    using taaproject.Services;
    using static taaproject.Services.ProjectService;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ProjectService _svc;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ProjectService svc)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _svc = svc;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _svc.GetAllAllowProjectAsync(User);
            return View(model.ToList());
        }

        public IActionResult Detail()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectModel model)
        {
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

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult Delete()
        {
            ViewData["Message"] = "Delete project completed.";

            return RedirectToAction("Index");
        }

        public IActionResult AddNewFeature()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewFeature(string Name, string Description)
        {
            var isValidData = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Name or Description can not be empty";
                return View();
            }

            return View(nameof(Detail));
        }

        public IActionResult AddNewStory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddNewStory(string Name, string Description)
        {
            var isValidData = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Name or Description can not be empty";
                return View();
            }

            return View(nameof(Detail));
        }

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

        public IActionResult EditFeature()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EditFeature(string Description, string AssignmentMember, string inlineRadioOptions)
        {
            var isValidData = !string.IsNullOrEmpty(Description) && !string.IsNullOrEmpty(AssignmentMember) && !string.IsNullOrEmpty(inlineRadioOptions);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Description or AssignmentMember or InlineRadioOptions can not be empty";
                return View();
            }

            return View(nameof(Detail));
        }

        public IActionResult EditStory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EditStory(string Description, string AssignmentMember, string inlineRadioOptions)
        {
            var isValidData = !string.IsNullOrEmpty(Description) && !string.IsNullOrEmpty(AssignmentMember) && !string.IsNullOrEmpty(inlineRadioOptions);
            if (!isValidData)
            {
                ViewBag.ErrorMessage = "Description or AssignmentMember or InlineRadioOptions can not be empty";
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
