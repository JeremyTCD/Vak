using System;

namespace Jering.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITimeService
    {
        /// <summary>
        /// 
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}
