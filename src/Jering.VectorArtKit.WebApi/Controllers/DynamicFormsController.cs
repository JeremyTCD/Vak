using Jering.DynamicForms;
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
        /// Json representation of <see cref="DynamicFormData"/> for <paramref name="formModelName"/> if <paramref name="formModelName"/> is the name of an existing view model.
        /// NotFound if <paramref name="formModelName"/> is not the name of an existing view model.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetDynamicForm(string formModelName)
        {
            Type type = Type.GetType($"Jering.VectorArtKit.WebApi.FormModels.{formModelName}FormModel");

            if(type == null)
            {
                JsonResult jsonResult = new JsonResult(null);
                jsonResult.StatusCode = (int)HttpStatusCode.NotFound;

                return jsonResult;
            }

            return Json(_dynamicFormsServices.GetDynamicForm(type));
        }
    }
}
