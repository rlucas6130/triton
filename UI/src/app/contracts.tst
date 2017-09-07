${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;
    using System.Text;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {
        settings.IncludeProject("Engine");
        settings.OutputFilenameFactory = (file) => { 
            var enumFile = file.Enums.FirstOrDefault();

            if(enumFile != null) {
                return enumFile.name + ".ts";
            }

            return file.Classes.First().name + ".ts";
        };
    }

    string Imports(Class @class) {
        var imports = new StringBuilder();

        if(@class.BaseClass != null) {
            imports.Append("import { "+@class.BaseClass+" } from \"./" + @class.BaseClass.name + "\";");
        }

        foreach(var property in @class.Properties) {
            if(!property.Type.IsPrimitive) {
                imports.Append("import { "+@property.Type.ClassName()+" } from \"./" + property.Type.ClassName() + "\"; \n");
            }
        }
        
        return imports.ToString();
    }

    string Extends(Class @class) {
        if(@class.BaseClass != null) {
            return "extends " + @class.BaseClass;
        }
        
        return null;
    }
}
$Classes(Engine.Contracts*)[$Imports
export interface $Name$TypeParameters $Extends
{ $Properties[
    $name: $Type;]
}
] 

$Enums(Engine.Contracts*)[
export enum $Name {$Values[
    $Name,]
}
]