using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Jering.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMemoryCacheService
    {
        object Get(object key);
        void Set<T>(object key, T value, MemoryCacheEntryOptions options);
    }
}
