﻿using Jering.Accounts.DatabaseInterface;
using Jering.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jering.Security
{
    /// <summary>
    /// Provides data protection token services.
    /// </summary>
    public class DataProtectionTokenService<TAccount> : ITokenService<TAccount> where TAccount : IAccount
    {
        private TokenServiceOptions _options { get; }

        private IDataProtector _dataProtector { get; }

        private ITimeService _timeService { get; }

        /// <summary>
        /// Constructs an instance of <see cref="DataProtectionTokenService{TAccount}"/> 
        /// </summary>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="timeService"></param>
        public DataProtectionTokenService(IDataProtectionProvider dataProtectionProvider,
            IOptions<TokenServiceOptions> optionsAccessor,
            ITimeService timeService)
        {
            _options = optionsAccessor.Value;
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(DataProtectionTokenService<TAccount>));
            _timeService = timeService;
        }

        /// <summary>
        /// Generates a Totp token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>A string representation of the generated token.</returns>
        public virtual string GenerateToken(string purpose, TAccount account)
        {
            if (purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            MemoryStream memoryStream = new MemoryStream();
            using (BinaryWriter binaryWriter = memoryStream.CreateWriter())
            {
                binaryWriter.Write(_timeService.UtcNow);
                binaryWriter.Write(account.AccountId);
                binaryWriter.Write(purpose);
                binaryWriter.Write(account.SecurityStamp.ToString());
            }
            byte[] protectedBytes = _dataProtector.Protect(memoryStream.ToArray());
            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        /// Validates the specified <paramref name="token"/> token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="ValidateTokenResult"/> with <see cref="ValidateTokenResult.Valid"/> set to true if token is valid.
        /// <see cref="ValidateTokenResult"/> with <see cref="ValidateTokenResult.Invalid"/> set to true if token is invalid.
        /// <see cref="ValidateTokenResult"/> with <see cref="ValidateTokenResult.Expired"/> set to true if token is expired.
        /// </returns>
        public virtual ValidateTokenResult ValidateToken(string purpose, string token, TAccount account)
        {
            if (purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            try
            {
                byte[] unprotectedBytes = _dataProtector.Unprotect(Convert.FromBase64String(token));
                MemoryStream memoryStream = new MemoryStream(unprotectedBytes);
                using (BinaryReader binaryReader = memoryStream.CreateReader())
                {
                    DateTimeOffset extractedCreationTime = binaryReader.ReadDateTimeOffset();
                    DateTimeOffset expirationTime = extractedCreationTime + _options.DataProtectionTokenLifespan;
                    if (expirationTime < _timeService.UtcNow)
                    {
                        return ValidateTokenResult.Expired;
                    }

                    int extractedAccountId = binaryReader.ReadInt32();
                    if (extractedAccountId != account.AccountId)
                    {
                        return ValidateTokenResult.Invalid;
                    }

                    string extractedPurpose = binaryReader.ReadString();
                    if (extractedPurpose != purpose)
                    {
                        return ValidateTokenResult.Invalid;
                    }

                    string extractedSecurityStamp = binaryReader.ReadString();
                    if (binaryReader.PeekChar() != -1 || extractedSecurityStamp != account.SecurityStamp.ToString())
                    {
                        return ValidateTokenResult.Invalid;
                    }

                    return ValidateTokenResult.Valid;
                }
            }
            catch
            {

            }

            return ValidateTokenResult.Invalid;
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
