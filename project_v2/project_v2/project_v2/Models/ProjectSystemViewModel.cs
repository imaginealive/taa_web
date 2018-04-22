using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectSystemViewModel
    {
        public List<ProjectRankModel> ranks { get; set; }
        public List<StatusModel> status { get; set; }
    }
}
