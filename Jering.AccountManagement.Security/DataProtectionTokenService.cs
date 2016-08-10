using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides data protection token services.
    /// </summary>
    public class DataProtectionTokenService<TAccount> : ITokenService<TAccount> where TAccount : IAccount
    {
        private AccountSecurityOptions _securityOptions { get; }

        private IDataProtector _dataProtector { get; }

        /// <summary>
        /// Constructs an instance of <see cref="DataProtectionTokenService{TAccount}"/> 
        /// </summary>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="securityOptionsAccessor"></param>
        public DataProtectionTokenService(IDataProtectionProvider dataProtectionProvider, IOptions<AccountSecurityOptions> securityOptionsAccessor)
        {
            _securityOptions = securityOptionsAccessor.Value;
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(DataProtectionTokenService<TAccount>));
        }

        /// <summary>
        /// Generates a Totp token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>A string representation of the generated token.</returns>
        public virtual Task<string> GenerateToken(string purpose, TAccount account)
        {
            return Task.Factory.StartNew(() =>
            {
                MemoryStream memoryStream = new MemoryStream();
                using (BinaryWriter binaryWriter = memoryStream.CreateWriter())
                {
                    binaryWriter.Write(DateTimeOffset.UtcNow);
                    binaryWriter.Write(account.AccountId);
                    binaryWriter.Write(purpose);
                    binaryWriter.Write(account.SecurityStamp.ToString());
                }
                byte[] protectedBytes = _dataProtector.Protect(memoryStream.ToArray());
                return Convert.ToBase64String(protectedBytes);
            });
        }

        /// <summary>
        /// Validates the specified <paramref name="token"/> token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <returns>
        /// True if <paramref name="token"/> is valid, false otherwise.
        /// </returns>
        public virtual Task<bool> ValidateToken(string purpose, string token, TAccount account)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    byte[] unprotectedBytes = _dataProtector.Unprotect(Convert.FromBase64String(token));
                    MemoryStream memoryStream = new MemoryStream(unprotectedBytes);
                    using (BinaryReader binaryReader = memoryStream.CreateReader())
                    {
                        DateTimeOffset extractedCreationTime = binaryReader.ReadDateTimeOffset();
                        DateTimeOffset expirationTime = extractedCreationTime + _securityOptions.TokenServiceOptions.DataProtectionTokenLifespan;
                        if (expirationTime < DateTimeOffset.UtcNow)
                        {
                            return false;
                        }

                        int extractedAccountId = binaryReader.ReadInt32();
                        if (extractedAccountId != account.AccountId)
                        {
                            return false;
                        }

                        string extractedPurpose = binaryReader.ReadString();
                        if (extractedPurpose != purpose)
                        {
                            return false;
                        }

                        string extractedSecurityStamp = binaryReader.ReadString();
                        if (binaryReader.PeekChar() != -1 || extractedSecurityStamp != account.SecurityStamp.ToString())
                        {
                            return false;
                        }

                        return true;
                    }
                }
                catch
                {
                    // Do not leak exception
                }
                return false;
            });
        }
    }

    /// <summary>
    /// Utility extensions to streams
    /// </summary>
    internal static class StreamExtensions
    {
        internal static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        public static BinaryReader CreateReader(this Stream stream)
        {
            return new BinaryReader(stream, DefaultEncoding, true);
        }

        public static BinaryWriter CreateWriter(this Stream stream)
        {
            return new BinaryWriter(stream, DefaultEncoding, true);
        }

        public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader)
        {
            return new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
        }

        public static void Write(this BinaryWriter writer, DateTimeOffset value)
        {
            writer.Write(value.UtcTicks);
        }
    }
}
