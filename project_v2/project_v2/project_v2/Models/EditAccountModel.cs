﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class EditAccountModel
    {
        public string _id { get; set; }
        
        [Display(Name = "ชื่อบัญผู้ใช้")]
        public string AccountName { get; set; }
        
        [Required(ErrorMessage = "กรุณากรอกชื่อ")]
        [Display(Name = "ชื่อ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกนามสกุล")]
        [Display(Name = "นามสกุล")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกอีเมล์")]
        [Display(Name = "อีเมล์")]
        public string Email { get; set; }

        [Display(Name = "ตำแหน่งงาน")]
        public string WorkPosition { get; set; }

        [Display(Name = "หน่วยงาน")]
        public string Department { get; set; }

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string Telephone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "วันเกิด")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "ผู้ใช้งานเป็น Admin ของระบบหรือไม่")]
        public bool IsAdmin { get; set; }

        [Display(Name = "ผู้ใช้งานสามารถสร้างโปรเจคในระบบได้หรือไม่")]
        public bool ProjectCreatable { get; set; }
        
        [Display(Name = "ระงับใช้งาน")]
        public bool IsSuspend { get; set; }
    }
}
