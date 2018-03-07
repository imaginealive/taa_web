using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taaproject.Models
{
    public interface IEmailSettings
    {
        String PrimaryDomain { get; set; }
        int PrimaryPort { get; set; }
        String SecondayDomain { get; set; }
        int SecondaryPort { get; set; }    
        String UsernameEmail { get; set; }
        String UsernamePassword { get; set; }
        String FromEmail { get; set; }
        String ToEmail { get; set; }
        String CcEmail { get; set; }
    }
}
