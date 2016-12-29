using System.Reflection;

namespace Jering.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class AssemblyService : IAssemblyService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        } 
    }
}
