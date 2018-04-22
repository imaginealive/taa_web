using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Controllers
{
    public class WorkController : Controller
    {
        private IProjectService projectSvc;
        private IFeatureService featureSvc;
        public WorkController(IProjectService projectSvc,IFeatureService featureSvc)
        {
            this.projectSvc = projectSvc;
            this.featureSvc = featureSvc;
        }

        public IActionResult CreateFeature(string projectid)
        {
            var model = projectSvc.GetProject(projectid);
            ViewBag.ProjectName = model.ProjectName;
            return View(new FeatureModel { Project_id = projectid });
        }

        [HttpPost]
        public IActionResult CreateFeature(FeatureModel model)
        {
            if (ModelState.IsValid)
            {
                featureSvc.CreateFeature(model);
                return RedirectToAction("Index", "Project", new { projectid = model.Project_id });
            }
            return View(model);
        }
    }
}