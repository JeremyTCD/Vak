using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public static class DynamicFormsServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddDynamicForms(this IServiceCollection services)
        {
               services.AddScoped<IDynamicFormsServices, DynamicFormsServices>();
        }
    }
}
