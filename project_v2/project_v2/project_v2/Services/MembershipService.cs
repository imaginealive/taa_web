using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;

namespace project_v2.Services
{
    public class MembershipService : Interface.IMembershipService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<ProjectMembershipModel> memberCollection;

        public MembershipService(IServiceConfigurations _svcConfig)
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
            memberCollection = database.GetCollection<ProjectMembershipModel>(svcConfig.MembershipCollection);
        }

        public void AddMember(string accountId, string projectId)
        {
            var IsValid = memberCollection
                .Find(it => it.Account_id == accountId && it.Project_id == projectId)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                var model = new ProjectMembershipModel
                {
                    _id = Guid.NewGuid().ToString(),
                    Account_id = accountId,
                    Project_id = projectId,
                    ProjectRank_id = svcConfig.GuestRankId,
                    CreateDate = DateTime.Now
                };
                memberCollection.InsertOne(model);
            }
        }

        public void EditMember(ProjectMembershipModel model)
        {
            memberCollection.FindOneAndUpdate(
                Builders<ProjectMembershipModel>.Filter.Eq(it => it._id, model._id),
                Builders<ProjectMembershipModel>.Update
                .Set(it => it.ProjectRank_id, model.ProjectRank_id)
                .Set(it => it.RemoveDate, model.RemoveDate)
            );
        }

        public List<ProjectMembershipModel> GetAllProjectMember(string projectId)
        {
            return memberCollection.Find(it => it.Project_id == projectId).ToList();
        }
    }
}
