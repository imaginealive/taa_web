using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Attribute
{
    public class AdminActionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Session.TryGetValue("LoginData", out byte[] text) || text.Length == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                var json = System.Text.Encoding.UTF8.GetString(text);
                var userdata = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountModel>(json);
                if (!userdata.IsAdmin)
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }
    }
}
