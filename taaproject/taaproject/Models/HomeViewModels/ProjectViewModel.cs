using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using taaproject.Models.WorkViewModels;

namespace taaproject.Models.HomeViewModels
{
    public class ProjectViewModel: Services.ProjectService.ProjectModel
    {
        public string ProjectOwner { get; set; }
        public List<FeatureViewModel> Work { get; set; }
    }
}
