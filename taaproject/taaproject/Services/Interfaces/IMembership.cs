using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taaproject.Models.Membership;
using static taaproject.Services.ProjectService;

namespace taaproject.Services.Interfaces
{
    public interface IMembership
    {
        Task<IEnumerable<MembershipInformation>> GetMemberships(string projectid);
        Task<MembershipModel> GetMembership(string projectid, string username);
        Task<bool> InviteMembership(string projectid, string username);
        Task<bool> RemoveMembership(string projectid, string username);
        Task<bool> ChangeMembershipInformation(MembershipModel request);
    }
}
