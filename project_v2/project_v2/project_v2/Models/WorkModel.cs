using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class WorkModel
    {
        public string _id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อ")]
        [Display(Name = "ชื่อ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
        [Display(Name = "รายละเอียด")]
        public string Description { get; set; }

        [Display(Name = "รายงานความคืบหน้า")]
        public string WorkReport { get; set; }

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreateDate { get; set; }

        [Required(ErrorMessage = "กรุณาระบุคาดการณ์วันที่งานต้องเสร็จสิ้น")]
        [Display(Name = "คาดการณ์วันที่งานต้องเสร็จสิ้น")]
        public DateTime ClosingDate { get; set; }

        [Display(Name = "สร้างโดย")]
        public string CreateByMember_id { get; set; }

        [Display(Name = "มอบหมายงานโดย")]
        public string AssginByMember_id { get; set; }

        [Display(Name = "ผู้รับมอบหมายหมาย")]
        public string BeAssignedMember_id { get; set; }

        [Display(Name = "สถานะของงาน")]
        public string StatusName { get; set; }

        [Display(Name = "วันที่งานเสร็จสิ้นจริง")]
        public DateTime? WorkDoneDate { get; set; }
    }
}
