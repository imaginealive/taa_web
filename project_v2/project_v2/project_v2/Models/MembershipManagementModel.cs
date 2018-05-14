using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class MembershipManagementModel
    {
        public string ProjectId { get; set; }
        public IEnumerable<DisplayMembership> Memberships { get; set; }
        public IEnumerable<AccountModel> NonMemberships { get; set; }
    }

    public class DisplayMembership : ProjectMembershipModel
    {
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string RankName { get; set; }
        public int AllWorkHasBeenAssigned { get; set; }

        public DisplayMembership()
        {

        }

        public DisplayMembership(ProjectMembershipModel model) : base()
        {
            this._id = model._id;
            this.Account_id = model.Account_id;
            this.Project_id = model.Project_id;
            this.ProjectRank_id = model.ProjectRank_id;
            this.CreateDate = model.CreateDate;
            this.RemoveDate = model.RemoveDate;
        }
    }
}
