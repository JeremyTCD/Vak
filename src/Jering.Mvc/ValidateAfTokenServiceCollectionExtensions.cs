using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jering.Mvc
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class ValidateAfTokenServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="ValidateAfTokenAuthorizationFilter"/> service.
        /// </summary>
        /// <param name="services"></param>
        public static void AddValidateAfToken(this IServiceCollection services)
        { 
            // Framework equivalanet, ValidateAntiForgeryTokenAuthorizationFilter, is added as singleton
            services.TryAddSingleton<ValidateAfTokenAuthorizationFilter>();
        }
    }
}
