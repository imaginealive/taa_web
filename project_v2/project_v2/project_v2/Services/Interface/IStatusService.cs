using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IStatusService
    {
        void AddStatus(StatusModel model);
        List<StatusModel> GetAllStatus();
        void EditStatus(StatusModel model);
        void DeleteStatus(string statusId);
    }
}
