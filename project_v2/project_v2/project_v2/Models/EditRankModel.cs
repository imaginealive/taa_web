using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class EditRankModel
    {
        public string ProjectId { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RankId { get; set; }
        public List<ProjectRankModel> Ranks { get; set; }

        [Display(Name = "สามารถเห็นงานทั้งหมดได้หรือไม่")]
        public bool CanSeeAllWork { get; set; }

        [Display(Name = "สามารถแก้ไขงานทั้งหมดได้หรือไม่")]
        public bool CanEditAllWork { get; set; }

        [Display(Name = "สามารถมอบหมายงานให้คนอื่นได้หรือไม่")]
        public bool CanAssign { get; set; }

        [Display(Name = "สามารถรับงานจากคนอื่นได้หรือไม่")]
        public bool BeAssigned { get; set; }

        [Display(Name = "สามารถสร้างงานหลักได้หรือไม่")]
        public bool CanCreateFeature { get; set; }

        [Display(Name = "สามารถสร้างงานรองต่อจากงานหลักได้หรือไม่")]
        public bool CanCreateStoryUnderSelf { get; set; }

        [Display(Name = "สามารถสร้างงานย่อยต่อจากงานรองได้หรือไม่")]
        public bool CanCreateTaskUnderSelf { get; set; }
    }
}
