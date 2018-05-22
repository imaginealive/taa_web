using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2
{
    public interface IServiceConfigurations
    {
        string ServerAddress { get; set; }
        int Port { get; set; }
        string DbUser { get; set; }
        string DbPassword { get; set; }
        string DatabaseName { get; set; }
        string ProjectCollection { get; set; }
        string AccountCollection { get; set; }
        string MembershipCollection { get; set; }
        string FeatureCollection { get; set; }
        string StoryCollection { get; set; }
        string TaskCollection { get; set; }
        string RankCollection { get; set; }
        string StatusCollection { get; set; }
        string AssignmentCollection { get; set; }
        string StatusNewId { get; set; }
        string GuestRankId { get; set; }
        string MasterRankId { get; set; }
    }
}
