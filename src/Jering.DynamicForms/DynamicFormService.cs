using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Jering.Utilities;

namespace Jering.DynamicForms
{
    public class DynamicFormService : IDynamicFormService
    {
        private IDynamicFormBuilder _dynamicFormBuilder { get; }
        private IMemoryCacheService _memoryCacheService { get; }
        private IAssemblyService _assemblyService { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="DynamicFormService"/>.
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="dynamicFormBuilder"></param>
        public DynamicFormService(IDynamicFormBuilder dynamicFormBuilder,
            IAssemblyService assemblyService,
            IMemoryCacheService memoryCache)
        {
            _dynamicFormBuilder = dynamicFormBuilder;
            _memoryCacheService = memoryCache;
            _assemblyService = assemblyService;
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

            DynamicFormData data = _memoryCacheService.Get(modelName) as DynamicFormData;

            if (data == null)
            {
                Assembly assembly = _assemblyService.GetEntryAssembly();
                Type type = assembly.
                    ExportedTypes.
                    First(exportedType => exportedType.Name == modelName);

                if (type != null)
                {
                    data = _dynamicFormBuilder.BuildDynamicFormData(type.GetTypeInfo());

                    if (data != null)
                    {
                        _memoryCacheService.
                            Set(modelName,
                                data,
                                new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove));
                    }
                }
            }

            return data;
        }
    }
}
