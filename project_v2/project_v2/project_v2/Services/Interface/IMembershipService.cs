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
        List<ProjectMembershipModel> GetAllProjectMember(string projectId);
        void EditMember(ProjectMembershipModel model);
    }
}
