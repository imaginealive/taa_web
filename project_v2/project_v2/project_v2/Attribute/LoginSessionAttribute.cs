using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Attribute
{
    public class LoginSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            byte[] text;
            if (!context.HttpContext.Session.TryGetValue("LoginData", out text) || text.Length == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
