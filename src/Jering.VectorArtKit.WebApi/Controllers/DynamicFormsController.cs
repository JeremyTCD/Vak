using Jering.AccountManagement.DatabaseInterface;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormsController : Controller
    {
        private IDynamicFormsService _dynamicFormsService {get;set;}
        private IAccountRepository<VakAccount> _vakAccountRepository;
        public DynamicFormsController(IAccountRepository<VakAccount> vakAccountRepository, 
            IDynamicFormsService dynamicFormsService)
        {
            _vakAccountRepository = vakAccountRepository;
            _dynamicFormsService = dynamicFormsService;
        }

        /// <summary>
        /// Gets <see cref="DynamicFormData"/> for a form model.
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

            return Ok(_dynamicFormsService.GetDynamicForm(type));
        }

        /// <summary>
        /// Validates that an email address is not in use.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// 200 ok with json representation of object with a single property. Property has key valid and value true if email is not in use, false otherwise.
        /// 400 bad request if <paramref name="value"/> is null.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [SetAntiForgeryToken]
        public async Task<IActionResult> ValidateEmailNotInUse(string value)
        {
            if(value == null)
            {
                return BadRequest();
            }
                       
            return Ok(JsonConvert.SerializeObject(new {valid = !await _vakAccountRepository.CheckEmailInUseAsync(value) }));
        }
    }
}
