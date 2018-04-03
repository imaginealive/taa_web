using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IMembershipService
    {
        void AddMember(string accountId, string projectId);
        ProjectMembershipModel GetAllProjectMember(string projectId);
        void ChangeMemberRank(string accountId, string projectId, string RankId);
        void RemoveMember(string accountId, string projectId);
    }
}
