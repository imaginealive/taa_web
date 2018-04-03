using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectRankModel
    {
        public string _id { get; set; }
        public string RankName { get; set; }
        public bool CanEditProject { get; set; }
        public bool CanSeeAllWork { get; set; }
        public bool CanEditAllWork { get; set; }
        public bool CanAssign { get; set; }
        public bool BeAssigned { get; set; }
        public bool CanManageMember { get; set; }
        public bool CanCreateFeature { get; set; }
        public bool CanCreateStoryUnderSelf { get; set; }
        public bool CanCreateTaskUnderSelf { get; set; }
    }
}
