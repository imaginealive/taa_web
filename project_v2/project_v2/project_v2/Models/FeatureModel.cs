using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class FeatureModel : WorkModel
    {
        public string Project_id { get; set; }
    }

    public class DisplayFeatureModel: FeatureModel
    {
        public string CreateByMemberName { get; set; }
        public string AssginByMemberName { get; set; }
        public string BeAssignedMemberName { get; set; }
        public string Status { get; set; }

        public DisplayFeatureModel(FeatureModel model)
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
            this.Project_id = model.Project_id;
        }
    }
}
