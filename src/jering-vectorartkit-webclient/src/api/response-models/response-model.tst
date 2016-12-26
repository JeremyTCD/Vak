${
	using Typewriter.Extensions.Types;
	using System.Text.RegularExpressions;

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
		if(interfaceName.Contains("ResponseModel") && !ImportedInterfaces.Contains(interfaceName))
		{
			ImportedInterfaces.Add(interfaceName);
			return $"import {{{interfaceName}}} from './{PascalToKebab(interfaceName)}';";
		}
		else
		{
			return null;
		}
	}
}$Classes(*ResponseModel)[$Properties[$ImportInterface] 

export interface $Name {$Properties[
	$name?: $Type;]
}]