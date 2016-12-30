using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Jering.VectorArtKit.WebApi.Extensions
{
    public static class IAntiforgeryExtensions
    {
        public static void AddAntiforgeryCookies(this IAntiforgery antiforgery, HttpContext context )
        {
            AntiforgeryTokenSet tokenSet = antiforgery.GetAndStoreTokens(context);
            context.
            Response.
            Cookies.
            Append(
                "XSRF-TOKEN",
                tokenSet.RequestToken,
                new CookieOptions()
                {
                    // Allows client side scripts to access cookie
                    HttpOnly = false
                });
        }
    }
}
