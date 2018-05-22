using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IAssignmentService
    {
        List<AssignmentModel> GetAssignments(string workId, WorkType type);
        void UpdateAssignment(string workId, string memberId, string status, WorkType type);
    }
}
