using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormsController : Controller
    {
        private IDynamicFormsServices _dynamicFormsServices {get;set;}

        public DynamicFormsController(IDynamicFormsServices dynamicFormsServices)
        {
            _dynamicFormsServices = dynamicFormsServices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formModelName"></param>
        /// <returns>
        /// 200 ok with json representation of <see cref="DynamicFormData"/> for <paramref name="formModelName"/> and validation forgery cookies if <paramref name="formModelName"/> is the name of an existing form model.
        /// 404 not found if <paramref name="formModelName"/> is not the name of an existing form model.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [SetAntiForgeryToken]
        public IActionResult GetDynamicForm(string formModelName)
        {
            Type type = Type.GetType($"Jering.VectorArtKit.WebApi.FormModels.{formModelName}FormModel");

            if(type == null)
            {
                return NotFound();
            }

            return Ok(_dynamicFormsServices.GetDynamicForm(type));
        }
    }
}
