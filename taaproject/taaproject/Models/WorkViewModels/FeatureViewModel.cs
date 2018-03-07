using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taaproject.Models.WorkViewModels
{
    public class FeatureViewModel : Services.WorkService.FeatureModel
    {
        public IEnumerable<StoryViewModel> StoryList { get; set; }
    }

    public class StoryViewModel : Services.WorkService.StoryModel
    {
        public IEnumerable<TaskViewModel> TaskList { get; set; }
    }

    public class TaskViewModel : Services.WorkService.TaskModel
    {
    }
}
