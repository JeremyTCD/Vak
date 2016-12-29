using System;
using System.Linq;
using System.Reflection;
using Jering.Utilities;
using System.Collections.Concurrent;

namespace Jering.DynamicForms
{
    public class DynamicFormService : IDynamicFormService
    {
        private ConcurrentDictionary<string, DynamicFormData> _formData { get; }
        private string _assemblyName { get; }
        private IDynamicFormBuilder _dynamicFormBuilder { get; }
        private IAssemblyService _assemblyService { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="DynamicFormService"/>.
        /// </summary>
        /// <param name="assemblyService"></param>
        /// <param name="dynamicFormBuilder"></param>
        public DynamicFormService(IDynamicFormBuilder dynamicFormBuilder,
            IAssemblyService assemblyService)
        {
            _dynamicFormBuilder = dynamicFormBuilder;
            _assemblyService = assemblyService;
            _formData = new ConcurrentDictionary<string, DynamicFormData>();
            _assemblyName = typeof(DynamicFormService).GetTypeInfo().Assembly.GetName().Name;
        }

        /// <summary>
        /// Gets <see cref="DynamicFormData"/> for model with name <paramref name="modelName"/>.
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> if a model with name <paramref name="modelName"/> exists and has a <see cref="DynamicFormAttribute"/>.
        /// null otherwise.
        /// </returns>
        public virtual DynamicFormData GetDynamicFormAction(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                throw new ArgumentException(nameof(modelName));
            }

            return _formData.GetOrAdd(modelName, FormDataGenerator);
        }


        #region Helpers
        private DynamicFormData FormDataGenerator(string modelName)
        {
            Type type = _assemblyService.
                GetReferencingAssemblies(_assemblyName).
                SelectMany(a => a.ExportedTypes).
                FirstOrDefault(e => e.Name == modelName);

            if (type != null)
            {
                return _dynamicFormBuilder.BuildDynamicFormData(type.GetTypeInfo());
            }

            return null;
        } 
        #endregion
    }
}
