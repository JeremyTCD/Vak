using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Get: /Error/Index
        /// </summary>
        /// <returns>
        /// 500 internal server error with no body.
        /// </returns>
        public IActionResult Index()
        {
            return StatusCode(500);
        }
    }
}
