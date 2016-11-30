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
        /// 200 OK with <see cref="DynamicFormResponseModel"/> and anti-forgery cookies if <paramref name="formModelName"/> is the name of an existing form model.
        /// 404 NotFound with <see cref="ErrorResponseModel"/> if <paramref name="formModelName"/> is not the name of an existing form model.
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
        /// 200 OK with <see cref="ValidateResponseModel"/> with <see cref="ValidateResponseModel.Valid"/> set to true if email is not in use and set to false 
        /// if email is in use. 
        /// 400 BadRequest with <see cref="ErrorResponseModel"/> if <paramref name="value"/> is null.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateEmailNotInUse(string value)
        {
            if(value == null)
            {
                return BadRequest();
            }

            return Ok(new ValidateResponseModel(){Valid = !await _vakAccountRepository.CheckEmailInUseAsync(value) });
        }
    }
}
