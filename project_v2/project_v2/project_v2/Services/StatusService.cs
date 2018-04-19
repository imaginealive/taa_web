using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using project_v2.Models;

namespace project_v2.Services
{
    public class StatusService : Interface.IStatusService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<StatusModel> statusCollection;

        public StatusService(IServiceConfigurations _svcConfig)
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
            statusCollection = database.GetCollection<StatusModel>(svcConfig.StatusCollection);
        }

        public void AddStatus(StatusModel model)
        {
            var IsValid = statusCollection
                .Find(it => it.StatusName == model.StatusName)
                .FirstOrDefault() == null;

            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.Deletable = true;
                statusCollection.InsertOne(model);
            }
        }

        public void DeleteStatus(string statusId)
        {
            statusCollection.DeleteOne(Builders<StatusModel>.Filter.Eq(it => it._id, statusId));
        }

        public void EditStatus( StatusModel model)
        {
            statusCollection.FindOneAndUpdate(
                Builders<StatusModel>.Filter.Eq(it => it._id, model._id),
                Builders<StatusModel>.Update
                .Set(it => it.StatusName, model.StatusName)
                .Set(it => it.IsWorkDone, model.IsWorkDone)
            );
        }

        public List<StatusModel> GetAllStatus()
        {
            return statusCollection.Find(it => true).ToList();
        }
    }
}
