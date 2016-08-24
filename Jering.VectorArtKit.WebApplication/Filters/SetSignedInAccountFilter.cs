using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace Jering.VectorArtKit.WebApplication.Filters
{
    public class SetSignedInAccountAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            IAccountSecurityServices<VakAccount> accountSecurityServices = (IAccountSecurityServices<VakAccount>) context.
                HttpContext.
                RequestServices.
                GetService(typeof(IAccountSecurityServices<VakAccount>));

            (context.Controller as Controller).ViewData.Add(
                nameof(VakAccount),
                accountSecurityServices.GetSignedInAccount());
        }
    }
}
