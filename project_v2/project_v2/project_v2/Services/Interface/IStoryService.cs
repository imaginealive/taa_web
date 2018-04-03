using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    interface IStoryService
    {
        void CreateStory(StoryModel model);
        List<StoryModel> GetStories(string featureId);
        StoryModel GetStory(string storyId);
        void EditStory(string storyId, StoryModel model);
        void DeleteStory(string storyId);
    }
}
