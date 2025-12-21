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
        }

        return dependencies.Distinct().ToList();
    }
}
