using System.Collections.Generic;
using System.Reflection;
using System;
using Jering.DynamicForms;

namespace Jering.VectorArtKit.WebApi.ResponseModels.DynamicForm
{
    /// <summary>
    /// Data that defines a dynamic form
    /// </summary>
    public class GetDynamicFormResponseModel
    {
        public DynamicFormData DynamicFormData { get; set; }
    }
}
