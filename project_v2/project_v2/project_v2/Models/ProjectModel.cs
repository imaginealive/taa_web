using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class ProjectModel
    {
        public string _id { get; set; }

        /// <summary>
        /// ชื่อโปรเจค
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกชื่อโปรเจค")]
        [Display(Name = "ชื่อโปรเจค")]
        public string ProjectName { get; set; }

        /// <summary>
        /// รายละเอียด
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
        [Display(Name = "รายละเอียด")]
        public string Description { get; set; }

        /// <summary>
        /// รูปภาพโปรเจค
        /// </summary>
        [Display(Name = "รูปภาพโปรเจค")]
        public string LogoUrl { get; set; }

        /// <summary>
        /// แผนก
        /// </summary>
        [Display(Name = "หน่วยงาน")]
        public string Department { get; set; }

        /// <summary>
        /// วันที่สร้างโปรเจค
        /// </summary>
        [Display(Name = "วันที่สร้างโปรเจค")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// วันที่เสร็จสิ้นโปรเจค
        /// </summary>
        [Required(ErrorMessage = "กรุณาระบุวันที่เสร็จสิ้นโปรเจค")]
        [Display(Name = "วันที่เสร็จสิ้นโปรเจค")]
        public DateTime ClosingDate { get; set; }
    }
}
