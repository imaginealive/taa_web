using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;
using project_v2.Services.Interface;

namespace project_v2.Services
{
    public class TaskService : Interface.ITaskService
    {
        IServiceConfigurations svcConfig;
        IAssignmentService assignSvc;
        IMongoCollection<TaskModel> taskCollection;
        IMongoCollection<StatusModel> statusCollection;

        public TaskService(IServiceConfigurations _svcConfig, IAssignmentService assignSvc)
        {
            this.assignSvc = assignSvc;
            svcConfig = _svcConfig;
            var cred = MongoCredential.CreateCredential(svcConfig.DatabaseName, svcConfig.DbUser, svcConfig.DbPassword);
            var sett = new MongoClientSettings
            {
                Server = new MongoServerAddress(svcConfig.ServerAddress, svcConfig.Port),
                Credentials = new List<MongoCredential> { cred }
            };
            MongoClient client = new MongoClient(sett);
            IMongoDatabase database = client.GetDatabase(svcConfig.DatabaseName);
            taskCollection = database.GetCollection<TaskModel>(svcConfig.TaskCollection);
            statusCollection = database.GetCollection<StatusModel>(svcConfig.StatusCollection);
        }

        public void CreateTask(TaskModel model)
        {
            var IsValid = taskCollection
                .Find(it => it.Name == model.Name)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.Now;
                model.WorkDoneDate = null;
                model.ClosingDate = DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local);
                model.StatusName = statusCollection.Find(it => it._id == svcConfig.StatusNewId).FirstOrDefault()?.StatusName;
                taskCollection.InsertOne(model);
            }
        }

        public void DeleteTask(string taskId)
        {
            taskCollection.DeleteOne(Builders<TaskModel>.Filter.Eq(it => it._id, taskId));
        }

        public void EditTask(TaskModel model)
        {
            var status = statusCollection.Find(it => it.StatusName == model.StatusName).FirstOrDefault();
            if (status != null && status.IsWorkDone)
                model.WorkDoneDate = DateTime.Now;
            else
                model.WorkDoneDate = null;

            taskCollection.FindOneAndUpdate(
                Builders<TaskModel>.Filter.Eq(it => it._id, model._id),
                Builders<TaskModel>.Update
                .Set(it => it.Name, model.Name)
                .Set(it => it.Description, model.Description)
                .Set(it => it.WorkReport, model.WorkReport)
                .Set(it => it.AssginByMember_id, model.AssginByMember_id)
                .Set(it => it.BeAssignedMember_id, model.BeAssignedMember_id)
                .Set(it => it.StatusName, model.StatusName)
                .Set(it => it.WorkDoneDate, model.WorkDoneDate)
                .Set(it => it.ClosingDate, DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local))
            );

            assignSvc.UpdateAssignment(model._id, model.BeAssignedMember_id, model.StatusName, WorkType.Task);
        }

        public List<TaskModel> GetTasks(string storyId)
        {
            return taskCollection.Find(it => it.Story_id == storyId).ToList();
        }
    }
}
