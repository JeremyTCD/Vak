using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Jering.Mvc
{
    /// <summary>
    /// Applies <see cref="ValidateAfTokenAuthorizationFilter"/> to an action or all actions in a controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateAfTokenAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        /// <summary>
        /// Filter must be executed after authentication
        /// </summary>
        public int Order { get; set; } = 1000;

        /// <summary>
        /// Inherited from <see cref="IFilterFactory"/> 
        /// </summary>
        public bool IsReusable => true;

        /// <summary>
        /// Inherited from <see cref="IFilterFactory"/> 
        /// </summary>
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ValidateAfTokenAuthorizationFilter>();
        }
    }
}