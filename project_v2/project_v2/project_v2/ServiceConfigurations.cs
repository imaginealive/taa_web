using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2
{
    public class ServiceConfigurations : IServiceConfigurations
    {
        public string DefaultConnection { get; set; }
        public string DatabaseName { get; set; }
        public string ProjectCollection { get; set; }
        public string AccountCollection { get; set; }
        public string MembershipCollection { get; set; }
        public string FeatureCollection { get; set; }
        public string StoryCollection { get; set; }
        public string TaskCollection { get; set; }
        public string RankCollection { get; set; }
        public string StatusCollection { get; set; }
        public string StatusNewId { get; set; }
        public string GuestRankId { get; set; }
        public string MasterRankId { get; set; }
    }
}
