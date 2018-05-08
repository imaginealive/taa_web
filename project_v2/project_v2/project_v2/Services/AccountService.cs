using MongoDB.Driver;
using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services
{
    public class AccountService : Interface.IAccountService
    {
        IServiceConfigurations svcConfig;
        IMongoCollection<AccountModel> accountCollection;

        public AccountService(IServiceConfigurations _svcConfig)
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
            accountCollection = database.GetCollection<AccountModel>(svcConfig.AccountCollection);
        }

        public void CreateAccount(AccountModel model)
        {
            var IsValid = accountCollection
                .Find(it => it.AccountName == model.AccountName)
                .FirstOrDefault() == null;
            
            if (IsValid)
            {
                model._id = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.Now;
                model.IsAdmin = false;
                model.ProjectCreatable = false;
                if (model.BirthDate.HasValue)
                    model.BirthDate = DateTime.SpecifyKind(model.BirthDate.Value.Date, DateTimeKind.Local);
                else
                    model.BirthDate = null;
                accountCollection.InsertOne(model);
            }
        }

        public void EditAccount(AccountModel model)
        {
            if (model.BirthDate.HasValue)
                model.BirthDate = DateTime.SpecifyKind(model.BirthDate.Value.Date, DateTimeKind.Local);
            else
                model.BirthDate = null;

            accountCollection.FindOneAndUpdate(
                Builders<AccountModel>.Filter.Eq(it => it._id, model._id),
                Builders<AccountModel>.Update
                .Set(it => it.Password, model.Password)
                .Set(it => it.FirstName, model.FirstName)
                .Set(it => it.LastName, model.LastName)
                .Set(it => it.BirthDate, model.BirthDate)
                .Set(it => it.WorkPosition, model.WorkPosition)
                .Set(it => it.Department, model.Department)
                .Set(it => it.Email, model.Email)
                .Set(it => it.Telephone, model.Telephone)
                .Set(it => it.IsAdmin, model.IsAdmin)
                .Set(it => it.ProjectCreatable, model.ProjectCreatable)
                .Set(it => it.SuspendDate, model.SuspendDate)
            );
        }

        public List<AccountModel> GetAllAccount()
        {
            return accountCollection.Find(it => true).ToList();
        }

        public AccountModel Login(string accountName, string password)
        {
            return accountCollection.Find(it => it.AccountName == accountName && it.Password == password && !it.SuspendDate.HasValue).FirstOrDefault();
        }
    }
}
