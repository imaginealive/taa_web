using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    interface IStatusService
    {
        void AddStatus(StatusModel model);
        List<StatusModel> GetAllStatus();
        StatusModel GetStatus(string statusId);
        void EditStatus(string statusId, StatusModel model);
        void DeleteStatus(string statusId);
    }
}
