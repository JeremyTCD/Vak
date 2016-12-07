using Jering.AccountManagement.DatabaseInterface;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Filters;
using Jering.VectorArtKit.WebApi.ResponseModels.DynamicForms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormsController : Controller
    {
        private IDynamicFormsBuilder _dynamicFormsBuilder {get;set;}
        private IAccountRepository<VakAccount> _vakAccountRepository;
        public DynamicFormsController(IAccountRepository<VakAccount> vakAccountRepository, 
            IDynamicFormsBuilder dynamicFormsBuilder)
        {
            _vakAccountRepository = vakAccountRepository;
            _dynamicFormsBuilder = dynamicFormsBuilder;
        }

        /// <summary>
        /// Gets <see cref="DynamicFormResponseModel"/> for a form model.
        /// </summary>
        /// <param name="formModelName"></param>
        /// <returns>
        /// 200 OK , <see cref="DynamicFormResponseModel"/> and anti-forgery cookies if <paramref name="formModelName"/> is the name of an existing form model.
        /// 404 NotFound and <see cref="ErrorResponseModel"/> if <paramref name="formModelName"/> is not the name of an existing form model.
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

            return Ok(_dynamicFormsBuilder.BuildDynamicFormResponseModel(type.GetTypeInfo()));
        }

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

            return Ok(new ValidateResponseModel(){Valid = !await _vakAccountRepository.CheckEmailInUseAsync(value) });
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

            return Ok(new ValidateResponseModel() { Valid = !await _vakAccountRepository.CheckDisplayNameInUseAsync(value) });
        }
    }
}
