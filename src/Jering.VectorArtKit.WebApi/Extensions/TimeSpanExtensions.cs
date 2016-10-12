using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Extensions
{
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Creates a string describing time elapsed over <paramref name="elapsedDuration"/>.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToElapsedDurationString(this TimeSpan elapsedDuration)
        {
            if (elapsedDuration.Days >= 365)
            {
                return "more than a year ago";
            }
            else if (elapsedDuration.Days >= 30)
            {
                int months = elapsedDuration.Days / 30;
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }
            else if (elapsedDuration.Days >= 1)
            {
                return $"{elapsedDuration.Days} day{(elapsedDuration.Days == 1 ? "" : "s")} ago";
            }
            else if (elapsedDuration.Hours >= 1)
            {
                return $"{elapsedDuration.Hours} hour{(elapsedDuration.Hours == 1 ? "" : "s")} ago";
            }
            else if (elapsedDuration.Minutes >= 1)
            {
                return $"{elapsedDuration.Minutes} minute{(elapsedDuration.Minutes == 1 ? "" : "s")} ago";
            }
            else
            {
                return "just now";
            }
        }
    }
}
