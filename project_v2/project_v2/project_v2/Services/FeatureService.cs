using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Services
{
    public class FeatureService : Interface.IFeatureService
    {
        IServiceConfigurations svcConfig;
        IAssignmentService assignSvc;
        IMongoCollection<FeatureModel> featureCollection;
        IMongoCollection<StoryModel> storyCollection;
        IMongoCollection<TaskModel> taskCollection;
        IMongoCollection<StatusModel> statusCollection;

        public FeatureService(IServiceConfigurations _svcConfig, IAssignmentService assignSvc)
        {
            svcConfig = _svcConfig;
            this.assignSvc = assignSvc;
            var cred = MongoCredential.CreateCredential(svcConfig.DatabaseName, svcConfig.DbUser, svcConfig.DbPassword);
            var sett = new MongoClientSettings
            {
                Server = new MongoServerAddress(svcConfig.ServerAddress, svcConfig.Port),
                Credentials = new List<MongoCredential> { cred }
            };
            MongoClient client = new MongoClient(sett);
            IMongoDatabase database = client.GetDatabase(svcConfig.DatabaseName);
            featureCollection = database.GetCollection<FeatureModel>(svcConfig.FeatureCollection);
            storyCollection = database.GetCollection<StoryModel>(svcConfig.StoryCollection);
            taskCollection = database.GetCollection<TaskModel>(svcConfig.TaskCollection);
            statusCollection = database.GetCollection<StatusModel>(svcConfig.StatusCollection);
        }

        public void CreateFeature(FeatureModel model)
        {
            var IsValid = featureCollection
                .Find(it => it.Name == model.Name)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.Now;
                model.WorkDoneDate = null;
                model.ClosingDate = DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local);
                model.StatusName = statusCollection.Find(it => it._id == svcConfig.StatusNewId).FirstOrDefault()?.StatusName;
                featureCollection.InsertOne(model);
            }
        }

        public void DeleteFeature(string featureId)
        {
            var storyList = storyCollection.Find(it => it.Feature_id == featureId).ToList();
            foreach (var item in storyList)
            {
                taskCollection.DeleteMany(Builders<TaskModel>.Filter.Eq(it => it.Story_id, item._id));
            }
            storyCollection.DeleteMany(Builders<StoryModel>.Filter.Eq(it => it.Feature_id, featureId));
            featureCollection.DeleteOne(Builders<FeatureModel>.Filter.Eq(it => it._id, featureId));
        }

        public void EditFeature(FeatureModel model)
        {
            var status = statusCollection.Find(it => it.StatusName == model.StatusName).FirstOrDefault();
            if (status != null && status.IsWorkDone)
                model.WorkDoneDate = DateTime.Now;
            else
                model.WorkDoneDate = null;

            featureCollection.FindOneAndUpdate(
                Builders<FeatureModel>.Filter.Eq(it => it._id, model._id),
                Builders<FeatureModel>.Update
                .Set(it => it.Name, model.Name)
                .Set(it => it.Description, model.Description)
                .Set(it => it.WorkReport, model.WorkReport)
                .Set(it => it.AssginByMember_id, model.AssginByMember_id)
                .Set(it => it.BeAssignedMember_id, model.BeAssignedMember_id)
                .Set(it => it.StatusName, model.StatusName)
                .Set(it => it.WorkDoneDate, model.WorkDoneDate)
                .Set(it => it.ClosingDate, DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local))
            );

            assignSvc.UpdateAssignment(model._id, model.BeAssignedMember_id, model.StatusName, WorkType.Feature);
        }

        public List<FeatureModel> GetFeatures(string projectId)
        {
            return featureCollection.Find(it => it.Project_id == projectId).ToList();
        }
    }
}
