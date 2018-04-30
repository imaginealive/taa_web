using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class EditRankModel
    {
        public string ProjectId { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RankId { get; set; }
        public List<ProjectRankModel> Ranks { get; set; }
    }
}
