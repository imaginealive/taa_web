using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class AssignmentModel
    {
        public string _id { get; set; }
        public WorkType Type { get; set; }
        public string Work_id { get; set; }
        public string Member_id { get; set; }
        public string LastestWorkStatus { get; set; }
        public DateTime AssignDate { get; set; }
        public DateTime? AbandonDate { get; set; }
    }

    public enum WorkType
    {
        Feature, Story, Task
    }
}
