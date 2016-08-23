﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Jering.VectorArtKit.WebApplication.Filters;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Get: /Home/Index
        /// </summary>
        /// <returns>
        /// Home index view with logoff form, when an account is logged in.
        /// Home index view with login form, when no account is logged in.
        /// </returns>
        [ServiceFilter(typeof(SetSignedInAccountFilter))]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
