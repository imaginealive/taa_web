using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static taaproject.Services.ProjectService;

namespace taaproject.Models.Membership
{
    public class MembershipInformation : MembershipModel
    {
        public string Email { get; set; }
    }
}
