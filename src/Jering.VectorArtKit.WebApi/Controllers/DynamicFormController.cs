using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Extensions;
using Jering.VectorArtKit.WebApi.RequestModels.DynamicForm;
using Jering.VectorArtKit.WebApi.ResponseModels.DynamicForm;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    public class DynamicFormController : Controller
    {
        private IDynamicFormBuilder _dynamicFormsBuilder {get;set;}
        private IDynamicFormService _dynamicFormService { get; set; }

        public DynamicFormController(IDynamicFormBuilder dynamicFormsBuilder,
            IDynamicFormService dynamicFormService)
        {
            _dynamicFormsBuilder = dynamicFormsBuilder;
            _dynamicFormService = dynamicFormService;
        }

        /// <summary>
        /// Gets <see cref="GetDynamicFormResponseModel"/> for a form model.
        /// </summary>
        /// <param name="requestModelName"></param>
        /// <returns>
        /// 200 OK and <see cref="GetDynamicFormResponseModel"/> if request model name is the name of an existing model 
        /// with a <see cref="DynamicFormAttribute"/>.
        /// 404 NotFound and <see cref="ErrorResponseModel"/> if request model name is not the name of 
        /// an existing model with a <see cref="DynamicFormAttribute"/>.
        /// </returns>
        [HttpGet]
        public IActionResult GetDynamicForm(GetDynamicFormRequestModel model)
        {
            DynamicFormData data = _dynamicFormService.GetDynamicFormAction(model.requestModelName);

            if(data == null)
            {
                return NotFound();
            }

            return Ok(new GetDynamicFormResponseModel()
            {
                DynamicFormData = data
            });
        }
    }
}
