using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Extensions;
using Jering.VectorArtKit.WebApi.RequestModels.DynamicForm;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormController : Controller
    {
        private IDynamicFormsBuilder _dynamicFormsBuilder {get;set;}
        private IAntiforgery _antiforgery;

        public DynamicFormController(IDynamicFormsBuilder dynamicFormsBuilder,
            IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
            _dynamicFormsBuilder = dynamicFormsBuilder;
        }

        // Logic should be moved to service

        /// <summary>
        /// Gets <see cref="DynamicFormResponseModel"/> for a form model.
        /// </summary>
        /// <param name="requestModelName"></param>
        /// <returns>
        /// 200 OK, anti-forgery cookies if getAfTokens is true and <see cref="DynamicFormResponseModel"/> if request model name is 
        /// the name of an existing dynamic form model.
        /// 404 NotFound and <see cref="ErrorResponseModel"/> if request model name is not the name of an existing dynamic form model.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetDynamicForm(GetDynamicFormRequestModel model)
        {
            // TODO cannot be account specific 
            Type type = Type.GetType($"Jering.VectorArtKit.WebApi.RequestModels.Account.{model.requestModelName}RequestModel");

            if(type == null)
            {
                return NotFound();
            }

            if (model.getAfTokens)
            {
                _antiforgery.AddAntiforgeryCookies(HttpContext);
            }

            return Ok(_dynamicFormsBuilder.BuildDynamicFormResponseModel(type.GetTypeInfo()));
        }
    }
}
