${
	using Typewriter.Extensions.Types;
	using System.Text.RegularExpressions;

    Template(Settings settings)
    {
        settings.
			IncludeProject("Jering.VectorArtKit.WebApi");

		settings.OutputFilenameFactory = file => {
				return PascalToKebab($"{file.Classes.First().Name}");
			};
    }

	static string PascalToKebab(string value){
		string extensionAdded = value + ".relative-urls.ts";
		string dashesAdded = Regex.Replace(
					extensionAdded,
					"(?<!^)([A-Z])",
					"-$1",
					RegexOptions.Compiled);

		return dashesAdded
				.Trim()
				.ToLower();
	}

    string RemoveController(Class controller){
        return controller.Name.Replace("Controller", "");
	}

    string ClassName(Class controller){
        return controller.Name + "RelativeUrls";
    }
}

$Classes(*Controller)[
export class $ClassName  {
    $Methods[
    static $name: string = `$Parent[$RemoveController]/$Name`;]
    ]
}
