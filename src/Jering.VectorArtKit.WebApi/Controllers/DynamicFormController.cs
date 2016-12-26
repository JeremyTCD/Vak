﻿using Jering.Accounts.DatabaseInterface;
using Jering.DynamicForms;
using Jering.VectorArtKit.DatabaseInterface;
using Jering.VectorArtKit.WebApi.Extensions;
using Jering.VectorArtKit.WebApi.RequestModels.DynamicForm;
using Jering.VectorArtKit.WebApi.ResponseModels.DynamicForms;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormController : Controller
    {
        private IDynamicFormsBuilder _dynamicFormsBuilder {get;set;}
        private IAccountRepository<VakAccount> _vakAccountRepository;
        private IAntiforgery _antiforgery;

        public DynamicFormController(IAccountRepository<VakAccount> vakAccountRepository, 
            IDynamicFormsBuilder dynamicFormsBuilder,
            IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
            _vakAccountRepository = vakAccountRepository;
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
            Type type = Type.GetType($"Jering.VectorArtKit.WebApi.RequestModels.{model.requestModelName}RequestModel");

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

        // These two belong in account controller

        /// <summary>
        /// Validates that an email address is not in use.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// 200 OK and <see cref="ValidateResponseModel"/> if value is not null or an empty string.
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateEmailNotInUse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest();
            }

            return Ok(new ValidateResponseModel(){Valid = !await _vakAccountRepository.
                CheckEmailInUseAsync(value, HttpContext.RequestAborted) });
        }


        /// <summary>
        /// Validates that a display name is not in use.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// 200 OK and <see cref="ValidateResponseModel"/> if <paramref name="value"/> is not null or an empty string.
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateDisplayNameNotInUse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest();
            }

            return Ok(new ValidateResponseModel() { Valid = !await _vakAccountRepository.
                CheckDisplayNameInUseAsync(value, HttpContext.RequestAborted) });
        }
    }
}