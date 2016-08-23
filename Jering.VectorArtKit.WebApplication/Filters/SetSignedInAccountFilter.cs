using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace Jering.VectorArtKit.WebApplication.Filters
{
    public class SetSignedInAccountFilter : IActionFilter
    {
        private IAccountSecurityServices<VakAccount> _accountSecurityServices { get; set; }

        public SetSignedInAccountFilter(IAccountSecurityServices<VakAccount> accountSecurityServices)
        {
            _accountSecurityServices = accountSecurityServices;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            (context.Controller as Controller).ViewData.Add(
                nameof(VakAccount),
                _accountSecurityServices.GetSignedInAccount());
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
