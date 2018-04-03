using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    interface ITaskService
    {
        void CreateTask(TaskModel model);
        List<TaskModel> GetTasks(string storyId);
        TaskModel GetTask(string taskId);
        void EditTask(string taskId, TaskModel model);
        void DeleteTask(string taskId);
    }
}
