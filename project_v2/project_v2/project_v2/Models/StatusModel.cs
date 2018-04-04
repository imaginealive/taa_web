using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class StatusModel
    {
        public string _id { get; set; }
        public string StatusName { get; set; }
        public bool IsWorkDone { get; set; }
        public bool Deletable { get; set; }
    }
}
