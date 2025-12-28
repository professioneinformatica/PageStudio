using Esprima.Ast;

namespace PageStudio.Core.Features.ParametricProperties;

public record PropertyDependency(string SymbolName, string PropertyName);

public static class DependencyExtractor
{
    public static List<PropertyDependency> Extract(Script script)
    {
        var dependencies = new List<PropertyDependency>();

        foreach (var node in script.DescendantNodes())
        {
            if (node is StaticMemberExpression { Object: Identifier objId, Property: Identifier propId })
            {
                dependencies.Add(new PropertyDependency(objId.Name, propId.Name));
            }
            // Supporto per ID normalizzati [[guid]]:property (che in JS diventano oggetti con ID come nome)
            else if (node is StaticMemberExpression { Object: Identifier { Name: var name }, Property: Identifier propId2 } && name.StartsWith("__id_"))
            {
                var guidStr = name.Substring(5).Replace("_", "-");
                if (Guid.TryParse(guidStr, out var id))
                {
                    dependencies.Add(new PropertyDependency(name, propId2.Name));
                }
            }
        }

        return dependencies.Distinct().ToList();
    }
}
