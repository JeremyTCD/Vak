using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class DynamicFormsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IDynamicFormsServices"/> service.
        /// </summary>
        /// <param name="services"></param>
        public static void AddDynamicForms(this IServiceCollection services)
        {
            services.AddScoped<IDynamicFormsServices, DynamicFormsServices>();
            services.AddScoped<IDynamicFormsBuilder, DynamicFormsBuilder>();
        }
    }
}
