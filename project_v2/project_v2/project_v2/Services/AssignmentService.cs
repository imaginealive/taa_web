using MongoDB.Driver;
using project_v2.Models;
using project_v2.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services
{
    public class AssignmentService : IAssignmentService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<AssignmentModel> assignmentCollection;

        public AssignmentService(IServiceConfigurations _svcConfig)
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
            assignmentCollection = database.GetCollection<AssignmentModel>(svcConfig.AssignmentCollection);
        }

        public List<AssignmentModel> GetAssignments(string workId, WorkType type) => assignmentCollection.Find(it => it.Work_id == workId && it.Type == type).ToList();

        public void UpdateAssignment(string workId, string memberId, string status, WorkType type)
        {
            var assign = assignmentCollection.Find(it => it.Work_id == workId && !it.AbandonDate.HasValue).FirstOrDefault();
            AssignmentModel data;

            if (assign == null && string.IsNullOrEmpty(memberId))
                return;

            if (assign == null && !string.IsNullOrEmpty(memberId))
            {
                data = new AssignmentModel
                {
                    _id = Guid.NewGuid().ToString(),
                    Work_id = workId,
                    Member_id = memberId,
                    AssignDate = DateTime.Now,
                    LastestWorkStatus = status,
                    Type = type
                };
                assignmentCollection.InsertOne(data);
            }
            else
            {
                data = assign;
                data.LastestWorkStatus = status;

                if (assign.Member_id != memberId)
                {
                    assignmentCollection.FindOneAndUpdate(
                        Builders<AssignmentModel>.Filter.Eq(it => it._id, data._id),
                        Builders<AssignmentModel>.Update
                        .Set(it => it.LastestWorkStatus, data.LastestWorkStatus)
                        .Set(it => it.AbandonDate, DateTime.Now)
                    );

                    data = new AssignmentModel
                    {
                        _id = Guid.NewGuid().ToString(),
                        Work_id = workId,
                        Member_id = memberId,
                        AssignDate = DateTime.Now,
                        LastestWorkStatus = status,
                        Type = type
                    };

                    assignmentCollection.InsertOne(data);
                }
                else
                {
                    assignmentCollection.FindOneAndUpdate(
                        Builders<AssignmentModel>.Filter.Eq(it => it._id, data._id),
                        Builders<AssignmentModel>.Update
                        .Set(it => it.LastestWorkStatus, data.LastestWorkStatus)
                    );
                }
            }
        }
    }
}
