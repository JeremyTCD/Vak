${
	using Typewriter.Extensions.Types;
	using System.Text.RegularExpressions;

    static string debugInfo = "";
    string PrintDebugInfo(Class c){
          return debugInfo;        
     }

    Template(Settings settings)
    {
        settings.
			IncludeProject("Jering.VectorArtKit.WebApi").
            IncludeProject("Jering.DynamicForms");

		settings.OutputFilenameFactory = file => {
				return PascalToKebab($"{file.Classes.First().Name}.ts");
			};
    }

	static string PascalToKebab(string value){
		string extensionAdded = value.Replace("ResponseModel", ".response-model");
		string dashesAdded = Regex.Replace(
					extensionAdded,
					"(?<!^)([A-Z])",
					"-$1",
					RegexOptions.Compiled);

		return dashesAdded
				.Trim()
				.ToLower();
	}

	static List<string> ImportedInterfaces = new List<string>();

	// Adds import statement for property type if necessary 
	static string ImportInterface(Property property){
		string interfaceName = TypeExtensions.ClassName(property.Type);
		if(!ImportedInterfaces.Contains(interfaceName))
		{
			ImportedInterfaces.Add(interfaceName);
			return $"import {{{interfaceName}}} from './{PascalToKebab(interfaceName)}';";
		}
		else
		{
			return null;
		}
	}

    static List<Type> AddedTypes = new List<Type>();

    IEnumerable<Type> DefinedTypes(Class c) 
    {
        AddedTypes.Clear();

        List<Type> result = new List<Type>();

        GetDependencies(c, null, result);

        return result;
    }

    void GetDependencies(Class root, Type type, List<Type> types){
        // Prevent duplicate interfaces

        if(AddedTypes.Any(t => type != null && t.Name == type.Name)){
            return;
        }
        else if(type != null)
        {
            AddedTypes.Add(type);
            types.Add(type);
        }
        
        PropertyCollection properties = type == null ? root.Properties : type.Properties; 

        IEnumerable<Type> dependencies = properties.
            Where(p => p.Type.Unwrap().IsDefined && !p.Type.Unwrap().IsEnumerable).
            Select(p => p.Type.Unwrap());

        foreach(Type t in dependencies){         
            GetDependencies(null, t, types);
        }           
    }
}

$Classes(*ResponseModel)[
    export interface $Name {$Properties[
	    $name?: $Type;]
    }

    $DefinedTypes[
        export interface $Name { $Properties[
            $name?: $Type;]
        }]

        $PrintDebugInfo
]
