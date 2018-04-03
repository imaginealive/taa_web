using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IProjectService
    {
        List<ProjectModel> GetProjects();
        ProjectModel GetProject(string projectId);
        void EditProject(string projectid, ProjectModel model);
    }
}
