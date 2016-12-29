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
            services.AddScoped<IDynamicFormService, DynamicFormService>();
            services.AddScoped<IDynamicFormBuilder, DynamicFormBuilder>();

            // Misc
            services.AddScoped<IAssemblyService, AssemblyService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
        }
    }
}
