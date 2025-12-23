using System.Globalization;
using Esprima;
using Esprima.Ast;

namespace PageStudio.Core.Features.ParametricProperties;

public class JsFormula
{
    public string Expression { get; }
    public Script Ast { get; }
    public IReadOnlyList<PropertyDependency> Dependencies { get; }

    public bool IsExplicitFormula { get; }

    public JsFormula(string expression, bool isExplicitFormula = true)
    {
        Expression = expression;
        IsExplicitFormula = isExplicitFormula;
        
        var parser = new JavaScriptParser();
        try 
        {
            Ast = parser.ParseScript(expression);
        }
        catch (ParserException ex)
        {
            // If it's a simple number, wrap it to be a valid expression if needed, 
            // but usually a number is a valid script.
            throw new ArgumentException($"Invalid JS expression: {expression}", ex);
        }
        
        Dependencies = DependencyExtractor.Extract(Ast);
    }

    public static JsFormula FromConstant(object value)
    {
        var valStr = value switch
        {
            string s => $"\"{s}\"",
            bool b => b ? "true" : "false",
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            null => "null",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "undefined"
        };
        return new JsFormula(valStr, false);
    }
}
