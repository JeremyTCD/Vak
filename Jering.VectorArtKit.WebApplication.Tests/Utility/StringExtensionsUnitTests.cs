using Jering.VectorArtKit.WebApplication.Utility;
using System.Collections.Generic;
using Xunit;

namespace Jering.VectorArtKit.WebApplication.Controllers.IntegrationTests.Utility
{
    public class StringExtensionsUnitTests
    {
        [Theory]
        [MemberData(nameof(ToHexStringData))]
        public void ToHexString_ConvertsToHexString(string originalString, string hexString)
        {
            // Act
            string result = originalString.ToHexString();

            // Assert
            Assert.Equal(hexString, result);
        }

        public static IEnumerable<object[]> ToHexStringData()
        {
            yield return new object[] { "abc", "616263" };
            yield return new object[] { "ʪǘǲ", "CAAAC798C7B2" };
        }

        [Theory]
        [MemberData(nameof(HexStringToStringData))]
        public void HexStringToString_ConvertsToHexString(string hexString, string originalString)
        {
            // Act
            string result = hexString.HexStringToString();

            // Assert
            Assert.Equal(originalString, result);
        }

        public static IEnumerable<object[]> HexStringToStringData()
        {
            yield return new object[] { "616263", "abc"  };
            yield return new object[] { "CAAAC798C7B2", "ʪǘǲ" };
        }
    }
}
