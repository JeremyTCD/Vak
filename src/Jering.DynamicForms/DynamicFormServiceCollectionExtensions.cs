using Jering.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class DynamicFormServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IDynamicFormBuilder"/> service.
        /// </summary>
        /// <param name="services"></param>
        public static void AddDynamicForms(this IServiceCollection services)
        {
            // DynamicFormService lazily generates and caches DynamicFormData 
            services.AddSingleton<IDynamicFormService, DynamicFormService>();
            services.AddScoped<IDynamicFormBuilder, DynamicFormBuilder>();

            // Misc
            // AssemblyService caches referencing assemblies
            services.AddSingleton<IAssemblyService, AssemblyService>();
        }
    }
}
