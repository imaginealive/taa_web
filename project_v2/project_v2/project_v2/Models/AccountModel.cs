﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class AccountModel
    {
        public string _id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อบัญผู้ใช้")]
        [Display(Name = "ชื่อบัญผู้ใช้")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านบัญชีผู้ใช้")]
        [Display(Name = "รหัสผ่านบัญชีผู้ใช้")]
        public string Password { get; set; }

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

        [Display(Name = "เบอร์โทรศัพท์")]
        public string Telephone { get; set; }

        [Display(Name = "วันเกิด")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "ผู้ใช้งานเป็น Admin ของระบบหรือไม่")]
        public bool IsAdmin { get; set; }

        [Display(Name = "ผู้ใช้งานสามารถโปรเจคในระบบได้หรือไม่")]
        public bool ProjectCreatable { get; set; }

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "วันที่ถูกระงับใช้งาน")]
        public DateTime? SuspendDate { get; set; }
    }
}
