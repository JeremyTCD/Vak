using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Jering.VectorArtKit.WebApplication.Filters;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Get: /Home/Index
        /// </summary>
        [SetSignedInAccount]
        public IActionResult Index()
        {
            return View();
        }
    }
}
