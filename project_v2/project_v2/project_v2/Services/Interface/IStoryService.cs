using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IStoryService
    {
        void CreateStory(StoryModel model);
        List<StoryModel> GetStories(string featureId);
        void EditStory(StoryModel model);
        void DeleteStory(string storyId);
    }
}
