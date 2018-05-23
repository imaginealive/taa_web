using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class StoryModel : WorkModel
    {
        public string Feature_id { get; set; }
    }

    public class DisplayStoryModel : StoryModel
    {
        public string CreateByMemberName { get; set; }
        public string AssginByMemberName { get; set; }
        public string BeAssignedMemberName { get; set; }
        
        public IEnumerable<DisplayTaskModel> Tasks { get; set; }
        public IEnumerable<DisplayAssignmentModel> assignmentHistories { get; set; }

        public DisplayStoryModel(StoryModel model)
        {
            this._id = model._id;
            this.Name = model.Name;
            this.Description = model.Description;
            this.WorkReport = model.WorkReport;
            this.CreateDate = model.CreateDate;
            this.ClosingDate = model.ClosingDate;
            this.CreateByMember_id = model.CreateByMember_id;
            this.AssginByMember_id = model.AssginByMember_id;
            this.BeAssignedMember_id = model.BeAssignedMember_id;
            this.StatusName = model.StatusName;
            this.WorkDoneDate = model.WorkDoneDate;
            this.Feature_id = model.Feature_id;
        }
    }
}
