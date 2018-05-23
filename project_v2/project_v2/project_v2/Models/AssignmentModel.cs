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

    public class DisplayAssignmentModel : AssignmentModel
    {
        public string MemberName { get; set; }

        public DisplayAssignmentModel(AssignmentModel model)
        {
            _id = model._id;
            Type = model.Type;
            Work_id = model.Work_id;
            Member_id = model.Work_id;
            LastestWorkStatus = model.LastestWorkStatus;
            AssignDate = model.AssignDate;
            AbandonDate = model.AbandonDate;
        }
    }

    public enum WorkType
    {
        Feature, Story, Task
    }
}
