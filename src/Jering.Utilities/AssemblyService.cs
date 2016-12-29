using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Concurrent;

namespace Jering.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class AssemblyService : IAssemblyService
    {
        private ConcurrentDictionary<string, IEnumerable<Assembly>> _referencingAssemblies { get; }

        /// <summary>
        /// Constructs a new instance of <see cref="AssemblyService"/> .
        /// </summary>
        public AssemblyService()
        {
            _referencingAssemblies = new ConcurrentDictionary<string, IEnumerable<Assembly>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public virtual IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            return _referencingAssemblies.GetOrAdd(assemblyName, ReferencingAssembliesGenerator);
        }

        #region Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public IEnumerable<Assembly> ReferencingAssembliesGenerator(string assemblyName)
        {
            List<Assembly> assemblies = new List<Assembly>();
            IReadOnlyList<RuntimeLibrary> dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (RuntimeLibrary library in dependencies)
            {
                if (IsCandidateLibrary(library, assemblyName))
                {
                    Assembly assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            return assemblies;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="library"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public bool IsCandidateLibrary(RuntimeLibrary library, string assemblyName)
        {
            return library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
        #endregion
    }
}
