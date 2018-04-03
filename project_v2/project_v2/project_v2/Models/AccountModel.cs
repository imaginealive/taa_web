using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class AccountModel
    {
        public string _id { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string WorkPosition { get; set; }
        public string Department { get; set; }
        public string Telephone { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool ProjectCreatable { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SuspendDate { get; set; }
    }
}
