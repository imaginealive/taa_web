using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IAccountService
    {
        void CreateAccount(AccountModel model);
        AccountModel Login(string accountName, string password);
        List<AccountModel> GetAllAccount();
        void EditAccount(AccountModel model);
    }
}
