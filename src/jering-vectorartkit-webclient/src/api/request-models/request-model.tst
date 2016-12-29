${
	using Typewriter.Extensions.Types;
	using System.Text.RegularExpressions;

    Template(Settings settings)
    {
        settings.
			IncludeProject("Jering.VectorArtKit.WebApi");

		settings.OutputFilenameFactory = file => {
				return PascalToKebab($"{file.Classes.First().Name}.ts");
			};
    }

	static string PascalToKebab(string value){
		string extensionAdded = value.Replace("RequestModel", ".request-model");
		string dashesAdded = Regex.Replace(
					extensionAdded,
					"(?<!^)([A-Z])",
					"-$1",
					RegexOptions.Compiled);

		return dashesAdded
				.Trim()
				.ToLower();
	}

    string nameName(Class c){
        return c.name + "Name";
    }
}$Classes(*RequestModel)[

export interface $Name {$Properties[
	$name?: string;]
}

export const $nameName: string = `$Name`;
]