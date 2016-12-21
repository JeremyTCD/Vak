using System;
using System.Collections.Generic;
using Xunit;

namespace Jering.Utilities.Tests.UnitTests
{
    public class TimeSpanExtensionsUnitTests
    {
        [Theory]
        [MemberData(nameof(ProcessData))]
        public void Process_ProducesExpectedOutput(string timeSpan, string expectedContent)
        {
            // Act
            string result = TimeSpan.Parse(timeSpan).ToElapsedDurationString();

            // Assert
            Assert.Equal(expectedContent, result);
        }

        public static IEnumerable<object[]> ProcessData()
        {
            yield return new object[] { "700.0:0:0", "more than a year ago" };
            yield return new object[] { "365.0:0:0", "more than a year ago" };
            yield return new object[] { "364.0:0:0", "12 months ago" };
            yield return new object[] { "30.0:0:0", "1 month ago" };
            yield return new object[] { "29.0:0:0", "29 days ago" };
            yield return new object[] { "1.0:0:0", "1 day ago" };
            yield return new object[] { "0.23:0:0", "23 hours ago" };
            yield return new object[] { "0.1:0:0", "1 hour ago" };
            yield return new object[] { "0.0:59:0", "59 minutes ago" };
            yield return new object[] { "0.0:1:0", "1 minute ago" };
            yield return new object[] { "0.0:0:59", "just now" };
        }
    }
}
