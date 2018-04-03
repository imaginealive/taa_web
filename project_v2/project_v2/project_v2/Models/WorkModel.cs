using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class WorkModel
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WorkReport { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateByMember_id { get; set; }
        public string AssginByMember_id { get; set; }
        public string BeAssignedMember_id { get; set; }
        public string Status_id { get; set; }
    }
}
