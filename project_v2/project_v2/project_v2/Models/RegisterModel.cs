using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Models
{
    public class RegisterModel : AccountModel
    {
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ComfirmPassword { get; set; }
    }
}
