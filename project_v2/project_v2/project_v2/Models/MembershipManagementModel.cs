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
        
    public class DisplayMembership: ProjectMembershipModel
    {
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string RankName { get; set; }
    }
}
