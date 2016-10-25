using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Filters
{
    public class SetAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();

            IAntiforgery antiForgery = (IAntiforgery) context.
                HttpContext.
                RequestServices.
                GetService(typeof(IAntiforgery));

            AntiforgeryTokenSet tokenSet = antiForgery.GetAndStoreTokens(context.HttpContext);
            context.
                HttpContext.
                Response.
                Cookies.
                Append(
                    "XSRF-TOKEN", 
                    tokenSet.RequestToken, 
                    new CookieOptions() {
                        // Allows client side scripts to access cookie
                        HttpOnly = false
                });
        }
    }
}
