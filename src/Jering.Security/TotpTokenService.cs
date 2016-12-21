using Jering.Accounts.DatabaseInterface;
using System;
using System.Globalization;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jering.Security
{
    /// <summary>
    /// Provides Time-based One Time Password (Totp) services.
    /// See https://tools.ietf.org/html/rfc6238
    /// </summary>
    public class TotpTokenService<TAccount> : ITokenService<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// Generates a Totp token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>A string representation of the generated token.</returns>
        public virtual string GenerateToken(string purpose, TAccount account)
        {
            if(purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            string modifier = GetTokenModifier(purpose, account);

            return GenerateTotp(account.SecurityStamp.ToByteArray(), modifier).ToString("D6", CultureInfo.InvariantCulture);
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
        /// </returns>
        public virtual ValidateTokenResult ValidateToken(string purpose, string token, TAccount account)
        {
            if(purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if(token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            int totp;
            if (!int.TryParse(token, out totp))
            {
                return ValidateTokenResult.Invalid;
            }

            string modifier = GetTokenModifier(purpose, account);

            return ValidateTotp(account.SecurityStamp.ToByteArray(), totp, modifier) ?
                ValidateTokenResult.Valid :
                ValidateTokenResult.Invalid;
        }

        /// <summary>
        /// Generates an arbitrarily constructed string from the specified <paramref name="purpose"/> and <paramref name="account"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>A string suitable for providing entropy in tokens.</returns>
        private string GetTokenModifier(string purpose, TAccount account)
        {
            return "Email:" + purpose + ":" + account.Email;
        }

        #region Totp helper methods
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan _timestep = TimeSpan.FromMinutes(3);
        private static readonly Encoding _encoding = new UTF8Encoding(false, true);

        private int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timestepNumber, string modifier)
        {
            // # of 0's = length of pin
            const int Mod = 1000000;

            // See https://tools.ietf.org/html/rfc4226
            // We can add an optional modifier
            var timestepAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timestepNumber));
            var hash = hashAlgorithm.ComputeHash(ApplyTotpModifier(timestepAsBytes, modifier));

            // Generate DT string
            var offset = hash[hash.Length - 1] & 0xf;
            Debug.Assert(offset + 4 < hash.Length);
            var binaryCode = (hash[offset] & 0x7f) << 24
                             | (hash[offset + 1] & 0xff) << 16
                             | (hash[offset + 2] & 0xff) << 8
                             | (hash[offset + 3] & 0xff);

            return binaryCode % Mod;
        }

        private byte[] ApplyTotpModifier(byte[] input, string totpModifier)
        {
            if (String.IsNullOrEmpty(totpModifier))
            {
                return input;
            }

            var modifierBytes = _encoding.GetBytes(totpModifier);
            var combined = new byte[checked(input.Length + modifierBytes.Length)];
            Buffer.BlockCopy(input, 0, combined, 0, input.Length);
            Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);
            return combined;
        }

        // More info: https://tools.ietf.org/html/rfc6238#section-4
        private ulong GetCurrentTimeStepNumber()
        {
            var delta = DateTime.UtcNow - _unixEpoch;
            return (ulong)(delta.Ticks / _timestep.Ticks);
        }

        private int GenerateTotp(byte[] securityToken, string modifier = null)
        {
            if (securityToken == null)
            {
                throw new ArgumentNullException(nameof(securityToken));
            }

            // Allow a variance of no greater than 90 seconds in either direction
            var currentTimeStep = GetCurrentTimeStepNumber();
            using (var hashAlgorithm = new HMACSHA1(securityToken))
            {
                return ComputeTotp(hashAlgorithm, currentTimeStep, modifier);
            }
        }

        private bool ValidateTotp(byte[] securityToken, int totp, string modifier = null)
        {
            if (securityToken == null)
            {
                throw new ArgumentNullException(nameof(securityToken));
            }

            // Allow a variance of no greater than 180 seconds in either direction
            var currentTimeStep = GetCurrentTimeStepNumber();
            using (var hashAlgorithm = new HMACSHA1(securityToken))
            {
                for (var i = -1; i <= 1; i++)
                {
                    var computedTotp = ComputeTotp(hashAlgorithm, (ulong)((long)currentTimeStep + i), modifier);
                    if (computedTotp == totp)
                    {
                        return true;
                    }
                }
            }

            // No match
            return false;
        }
        #endregion
    }
}
