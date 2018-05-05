using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class StatusModel
    {
        public string _id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสถานะ")]
        [Display(Name = "ชื่อสถานะ")]
        public string StatusName { get; set; }
        
        [Display(Name = "ถือว่างานเสร็จสิ้นหรือไม่")]
        public bool IsWorkDone { get; set; }
        public bool Deletable { get; set; }
    }
}
