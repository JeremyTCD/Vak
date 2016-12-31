using Jering.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class AntiForgeryController : Controller
    {
        private IAntiforgery _antiforgery { get; }

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        /// <summary>
        /// Gets anti forgery tokens wrapped in cookies.
        /// </summary>
        /// <returns>
        /// 200 OK and anti-forgery cookies.
        /// </returns>
        [HttpGet]
        public IActionResult GetAntiForgeryTokens()
        {
            _antiforgery.AddAntiforgeryCookies(HttpContext);

            return Ok();
        }
    }
}
