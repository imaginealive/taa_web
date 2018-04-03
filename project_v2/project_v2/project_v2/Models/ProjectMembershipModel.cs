using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectMembershipModel
    {
        public string _id { get; set; }
        public string Account_id { get; set; }
        public string Project_id { get; set; }
        public string ProjectRank_id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime RemoveDate { get; set; }
    }
}
