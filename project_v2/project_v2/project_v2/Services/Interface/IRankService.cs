using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    interface IRankService
    {
        void AddRank(ProjectRankModel model);
        List<ProjectRankModel> GetAllRank();
        ProjectRankModel GetRank(string rankId);
        void EditRank(string rankId, ProjectRankModel model);
        void DeleteRank(string rankId);
    }
}
