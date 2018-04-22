using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class RegisterModel : AccountModel
    {
        [Compare(nameof(Password), ErrorMessage = "รหัสผ่านบัญชีผู้ใช้ไม่ตรงกัน")]
        [Required(ErrorMessage = "ยืนยันรหัสผ่านบัญชีผู้ใช้ไม่สามารถว่างได้")]
        [Display(Name = "ยืนยันรหัสผ่านบัญชีผู้ใช้")]
        public string ComfirmPassword { get; set; }
    }
}
