using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace taaproject.Models.HomeViewModels
{
    public class ProjectViewModel: Services.ProjectService.ProjectModel
    {
        public string ProjectOwner { get; set; }
    }
}
