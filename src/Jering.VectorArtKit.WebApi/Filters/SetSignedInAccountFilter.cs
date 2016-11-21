using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Filters
{
    public class SetSignedInAccountAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();

            IAccountSecurityService<VakAccount> accountSecurityService = (IAccountSecurityService<VakAccount>) context.
                HttpContext.
                RequestServices.
                GetService(typeof(IAccountSecurityService<VakAccount>));

            (context.Controller as Controller).ViewData.Add(
                nameof(VakAccount),
                await accountSecurityService.GetLoggedInAccountAsync());
        }
    }
}
