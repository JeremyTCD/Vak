using Microsoft.Extensions.DependencyInjection;
using System;
using Jering.Accounts.DatabaseInterface;
using Jering.Security;

namespace Jering.Accounts
{
    public class AccountsBuilder
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
        private Type _accountType { get; }

        public AccountsBuilder(Type accountType, IServiceCollection serviceCollection)
        {
            _accountType = accountType;
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds an <see cref="IAccountRepository{TAccount}"/> service for the <seealso cref="_accountType"/>.
        /// </summary>
        /// <typeparam name="TAccountRepository">A type that implements <see cref="IAccountRepository{TAccount}"/>.</typeparam>
        /// <returns>The current <see cref="AccountsBuilder"/> instance.</returns>
        public virtual AccountsBuilder AddAccountRepository<TAccountRepository>() where TAccountRepository : class
        {
            return AddScoped(typeof(IAccountRepository<>).MakeGenericType(_accountType), typeof(TAccountRepository));
        }

        /// <summary>
        /// Adds an <see cref="IRoleRepository"/> service.
        /// </summary>
        /// <typeparam name="TRoleRepository">A type that implements <see cref="IRoleRepository"/>.</typeparam>
        /// <returns>The current <see cref="AccountsBuilder"/> instance.</returns>
        public virtual AccountsBuilder AddRoleRepository<TRoleRepository>() where TRoleRepository : class
        {
            return AddScoped(typeof(IRoleRepository), typeof(TRoleRepository));
        }

        /// <summary>
        /// Adds an <see cref="IClaimRepository"/> service.
        /// </summary>
        /// <typeparam name="TClaimRepository">A type that implements <see cref="IClaimRepository"/>.</typeparam>
        /// <returns>The current <see cref="AccountsBuilder"/> instance.</returns>
        public virtual AccountsBuilder AddClaimRepository<TClaimRepository>() where TClaimRepository : class
        {
            return AddScoped(typeof(IClaimRepository), typeof(TClaimRepository));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTokenServiceType"></typeparam>
        /// <param name="tokenServiceName"></param>
        /// <returns></returns>
        public virtual AccountsBuilder AddTokenService<TTokenServiceType>(string tokenServiceName)
        {
            AddTokenService(tokenServiceName, typeof(TTokenServiceType));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenServiceType"></param>
        /// <returns></returns>
        private AccountsBuilder AddTokenService(string tokenServiceName, Type tokenServiceType)
        {
            _serviceCollection.Configure<AccountsServiceOptions>(options =>
            {
                options.TokenServiceOptions.TokenServiceMap[tokenServiceName] = tokenServiceType;
            });

            AddScoped(tokenServiceType, tokenServiceType);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual AccountsBuilder AddDefaultTokenServices()
        {
            Type dataProtectionTokenServiceType = typeof(DataProtectionTokenService<>).MakeGenericType(_accountType);
            Type totpTokenServiceType = typeof(TotpTokenService<>).MakeGenericType(_accountType);

            return AddTokenService(TokenServiceOptions.DataProtectionTokenService, dataProtectionTokenServiceType).
                AddTokenService(TokenServiceOptions.TotpTokenService, totpTokenServiceType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="concreteType"></param>
        /// <returns></returns>
        private AccountsBuilder AddScoped(Type serviceType, Type concreteType)
        {
            _serviceCollection.AddScoped(serviceType, concreteType);
            return this;
        }
    }
}
