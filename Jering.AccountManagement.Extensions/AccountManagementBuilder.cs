using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;

namespace Jering.AccountManagement.Extensions
{
    public class AccountManagementBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> services are attached to.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> services are attached to.
        /// </value>
        private IServiceCollection _serviceCollection { get; }

        /// <summary>
        /// 
        /// </summary>
        private Type _userType { get; }

        public AccountManagementBuilder(Type userType, IServiceCollection serviceCollection)
        {
            _userType = userType;
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds an <see cref="IAccountRepository{TAccount}"/> for the <seealso cref="_userType"/>.
        /// </summary>
        /// <typeparam name="TAccountRepository">A type that implements <see cref="IAccountRepository{TAccount}"/>.</typeparam>
        /// <returns>The current <see cref="AccountManagementBuilder"/> instance.</returns>
        public virtual AccountManagementBuilder AddAccountRepository<TAccountRepository>() where TAccountRepository : class
        {
            return AddScoped(typeof(IAccountRepository<>).MakeGenericType(_userType), typeof(TAccountRepository));
        }

        public virtual AccountManagementBuilder AddTokenService<TTokenServiceType>(string tokenServiceName)
        {
            AddTokenService(tokenServiceName, typeof(TTokenServiceType));
            return this;
        }

        private AccountManagementBuilder AddTokenService(string tokenServiceName, Type tokenServiceType)
        {
            _serviceCollection.Configure<AccountSecurityOptions>(options =>
            {
                options.TokenServiceOptions.TokenServiceMap[tokenServiceName] = tokenServiceType;
            });

            AddScoped(tokenServiceType, tokenServiceType);
            return this;
        }

        public virtual AccountManagementBuilder AddDefaultTokenServices()
        {
            Type dataProtectionTokenServiceType = typeof(DataProtectionTokenService<>).MakeGenericType(_userType);
            Type totpTokenServiceType = typeof(TotpTokenService<>).MakeGenericType(_userType);

            return AddTokenService(TokenServiceOptions.DataProtectionTokenService, dataProtectionTokenServiceType).
                AddTokenService(TokenServiceOptions.TotpTokenService, totpTokenServiceType);
        }

        private AccountManagementBuilder AddScoped(Type serviceType, Type concreteType)
        {
            _serviceCollection.AddScoped(serviceType, concreteType);
            return this;
        }
    }
}
