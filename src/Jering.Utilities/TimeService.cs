using System;

namespace Jering.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class TimeService : ITimeService
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset UtcNow
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}
