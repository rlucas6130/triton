${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;
    using System.Text;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {
        settings.IncludeProject("UI");
        settings.OutputFilenameFactory = (file) => file.Classes.First().name + ".ts";
    }

    string Imports(Class @class) {
        var imports = new StringBuilder();

        if(@class.BaseClass != null) {
            imports.Append("import { "+@class.BaseClass+" } from \"./" + @class.BaseClass.name + "\";\n");
        }

        foreach(var property in @class.Properties) {
            if(!property.Type.IsPrimitive) {
                imports.Append("import { "+@property.Type.ClassName()+" } from \"./" + property.Type.ClassName() + "\"; \n");
            }

            if(property.Type.IsEnum) {
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

    string Nullable(Property property) {
        if(property.Type.IsNullable) {
            return "?";
        }
        
        return null;
    }
}
$Classes(UI.ViewModels.Dtos*)[$Imports
export interface $Name$TypeParameters $Extends
{ $Properties[
    $name$Nullable: $Type;]
}]