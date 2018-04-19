using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;

namespace project_v2.Services
{
    public class RankService : Interface.IRankService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<ProjectRankModel> rankCollection;
        IMongoCollection<ProjectMembershipModel> memberCollection;

        public RankService(IServiceConfigurations _svcConfig)
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
            rankCollection = database.GetCollection<ProjectRankModel>(svcConfig.RankCollection);
            memberCollection = database.GetCollection<ProjectMembershipModel>(svcConfig.MembershipCollection);
        }

        public void AddRank(ProjectRankModel model)
        {
            var IsValid = rankCollection
                .Find(it => it.RankName == model.RankName)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                rankCollection.InsertOne(model);
            }
        }

        public void DeleteRank(string rankId)
        {
            var IsValid = memberCollection
                .Find(it => it.ProjectRank_id == rankId)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                rankCollection.DeleteOne(Builders<ProjectRankModel>.Filter.Eq(it => it._id, rankId));
            }
        }

        public void EditRank(ProjectRankModel model)
        {
            rankCollection.FindOneAndUpdate(
                Builders<ProjectRankModel>.Filter.Eq(it => it._id, model._id),
                Builders<ProjectRankModel>.Update
                .Set(it => it.RankName, model.RankName)
                .Set(it => it.CanEditProject, model.CanEditProject)
                .Set(it => it.CanSeeAllWork, model.CanSeeAllWork)
                .Set(it => it.CanEditAllWork, model.CanEditAllWork)
                .Set(it => it.CanAssign, model.CanAssign)
                .Set(it => it.BeAssigned, model.BeAssigned)
                .Set(it => it.CanManageMember, model.CanManageMember)
                .Set(it => it.CanCreateFeature, model.CanCreateFeature)
                .Set(it => it.CanCreateStoryUnderSelf, model.CanCreateStoryUnderSelf)
                .Set(it => it.CanCreateTaskUnderSelf, model.CanCreateTaskUnderSelf)
            );
        }

        public List<ProjectRankModel> GetAllRank()
        {
            return rankCollection.Find(it => true).ToList();
        }
    }
}
