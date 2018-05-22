using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;

namespace project_v2.Services
{
    public class ProjectService : Interface.IProjectService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<ProjectModel> projectCollection;
        IMongoCollection<ProjectMembershipModel> memberCollection;

        public ProjectService(IServiceConfigurations _svcConfig)
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
            projectCollection = database.GetCollection<ProjectModel>(svcConfig.ProjectCollection);
            memberCollection = database.GetCollection<ProjectMembershipModel>(svcConfig.MembershipCollection);
        }

        public void CreateProject(string accountId, ProjectModel model)
        {
            var IsValid = projectCollection
                   .Find(it => it.ProjectName == model.ProjectName)
                   .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.Now;
                model.ClosingDate = DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local);
                projectCollection.InsertOne(model);

                var member = new ProjectMembershipModel
                {
                    _id = Guid.NewGuid().ToString(),
                    Account_id = accountId,
                    Project_id = model._id,
                    ProjectRank_id = svcConfig.MasterRankId,
                    CreateDate = DateTime.Now,
            };
                memberCollection.InsertOne(member);
            }
        }

        public void EditProject(ProjectModel model)
        {
            projectCollection.FindOneAndUpdate(
                Builders<ProjectModel>.Filter.Eq(it => it._id, model._id),
                Builders<ProjectModel>.Update
                .Set(it => it.ProjectName, model.ProjectName)
                .Set(it => it.Description, model.Description)
                .Set(it => it.Department, model.Department)
                .Set(it => it.LogoUrl, model.LogoUrl)
                .Set(it => it.WorkDoneDate, model.WorkDoneDate)
                .Set(it => it.ClosingDate, DateTime.SpecifyKind(model.ClosingDate, DateTimeKind.Local))
            );
        }

        public ProjectModel GetProject(string projectId)
        {
            return projectCollection.Find(it => it._id == projectId).FirstOrDefault();
        }

        public List<ProjectModel> GetProjects(string accountId)
        {
            var beMember = memberCollection.Find(it => it.Account_id == accountId).ToList();
            var project = projectCollection.Find(it => true).ToList();
            return project.Where(pj => beMember.Any(member => member.Project_id == pj._id && !member.RemoveDate.HasValue)).ToList();
        }
    }
}
