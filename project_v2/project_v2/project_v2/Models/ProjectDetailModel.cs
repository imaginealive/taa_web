using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectDetailModel
    {
        public ProjectModel Project { get; set; }
        public IEnumerable<DisplayMembership> Memberships { get; set; }
        public IEnumerable<FeatureModel> Features { get; set; }
    }
}
