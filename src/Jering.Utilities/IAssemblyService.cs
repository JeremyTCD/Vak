using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using System.Reflection;

namespace Jering.Utilities
{
    public interface IAssemblyService
    {
        IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName);
    }
}
