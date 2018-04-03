using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2
{
    public interface IServiceConfigurations
    {
        string DefaultConnection { get; set; }
        string DatabaseName { get; set; }
        string ProjectCollection { get; set; }
        string AccountCollection { get; set; }
        string MembershipCollection { get; set; }
        string FeatureCollection { get; set; }
        string StoryCollection { get; set; }
        string TaskCollection { get; set; }
        string RankCollection { get; set; }
        string StatusCollection { get; set; }
    }
}
