using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.Utility
{
    public static class StringExtensions
    {
        public static string ToHexString(this string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string HexStringToString(this string hS)
        {
            byte[] bytes = new byte[hS.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hS.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
