using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;

namespace project_v2.Services
{
    public class StoryService : Interface.IStoryService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<StoryModel> storyCollection;
        IMongoCollection<TaskModel> taskCollection;
        IMongoCollection<StatusModel> statusCollection;

        public StoryService(IServiceConfigurations _svcConfig)
        {
            svcConfig = _svcConfig;
            var cred = MongoCredential.CreateCredential(svcConfig.DatabaseName, svcConfig.DbUser, svcConfig.DbPassword);
            var sett = new MongoClientSettings
            {
                Server = new MongoServerAddress(svcConfig.ServerAddress, svcConfig.Port),
                Credentials = new List<MongoCredential> { cred }
            };
            MongoClient client = new MongoClient(sett);
            IMongoDatabase database = client.GetDatabase(svcConfig.DatabaseName);
            storyCollection = database.GetCollection<StoryModel>(svcConfig.StoryCollection);
            taskCollection = database.GetCollection<TaskModel>(svcConfig.TaskCollection);
            statusCollection = database.GetCollection<StatusModel>(svcConfig.StatusCollection);
        }

        public void CreateStory(StoryModel model)
        {
            var IsValid = storyCollection
                .Find(it => it.Name == model.Name)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.Now;
                model.WorkDoneDate = null;
                model.ClosingDate = DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local);
                model.StatusName = statusCollection.Find(it => it._id == svcConfig.StatusNewId).FirstOrDefault()?.StatusName;
                storyCollection.InsertOne(model);
            }
        }

        public void DeleteStory(string storyId)
        {
            taskCollection.DeleteMany(Builders<TaskModel>.Filter.Eq(it => it.Story_id, storyId));
            storyCollection.DeleteOne(Builders<StoryModel>.Filter.Eq(it => it._id, storyId));
        }

        public void EditStory(StoryModel model)
        {
            var status = statusCollection.Find(it => it.StatusName == model.StatusName).FirstOrDefault();
            if (status != null && status.IsWorkDone)
                model.WorkDoneDate = DateTime.Now;
            else
                model.WorkDoneDate = null;

            storyCollection.FindOneAndUpdate(
                Builders<StoryModel>.Filter.Eq(it => it._id, model._id),
                Builders<StoryModel>.Update
                .Set(it => it.Name, model.Name)
                .Set(it => it.Description, model.Description)
                .Set(it => it.WorkReport, model.WorkReport)
                .Set(it => it.AssginByMember_id, model.AssginByMember_id)
                .Set(it => it.BeAssignedMember_id, model.BeAssignedMember_id)
                .Set(it => it.StatusName, model.StatusName)
                .Set(it => it.WorkDoneDate, model.WorkDoneDate)
                .Set(it => it.ClosingDate, DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local))
            );
        }

        public List<StoryModel> GetStories(string featureId)
        {
            return storyCollection.Find(it => it.Feature_id == featureId).ToList();
        }
    }
}
