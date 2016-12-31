// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Jering.Mvc.Resources;

namespace Jering.Mvc
{
    /// <summary>
    /// Validates anti-forgery token
    /// </summary>
    public class ValidateAfTokenAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IAntiforgery _antiforgery;

        /// <summary>
        /// Creates an insance of <see cref="ValidateAfTokenAuthorizationFilter"/> .
        /// </summary>
        /// <param name="antiforgery"></param>
        public ValidateAfTokenAuthorizationFilter(IAntiforgery antiforgery)
        {
            if (antiforgery == null)
            {
                throw new ArgumentNullException(nameof(antiforgery));
            }

            _antiforgery = antiforgery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                await _antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponseModel { ExpectedError = true,
                        ErrorMessage = MvcStrings.ErrorMessage_InvalidAntiForgeryToken,
                        AntiForgeryError = true
                    });
            }
        }
    }
}