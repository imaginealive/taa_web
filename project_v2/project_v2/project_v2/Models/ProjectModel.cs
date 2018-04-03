using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectModel
    {
        public string _id { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Department { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
