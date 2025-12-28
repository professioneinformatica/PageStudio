using System.Globalization;
using System.Text.RegularExpressions;
using Esprima;
using Esprima.Ast;

namespace PageStudio.Core.Features.ParametricProperties;

public class JsFormula
{
    public string Expression { get; }
    public string OriginalExpression { get; }
    public Script Ast { get; }
    public IReadOnlyList<PropertyDependency> Dependencies { get; }

    public bool IsExplicitFormula { get; }

    public JsFormula(string expression, bool isExplicitFormula = true)
    {
        OriginalExpression = expression;
        
        // Se l'espressione contiene [[guid:prop]], la trasformiamo in __id_guid.prop per l'esecuzione JS
        var processedExpression = Regex.Replace(expression, @"\[\[([0-9a-fA-F-]{36}):([a-zA-Z_][a-zA-Z0-9_]*)\]\]", match => 
        {
            var guid = match.Groups[1].Value.Replace("-", "_");
            var prop = match.Groups[2].Value;
            return "__id_" + guid + "." + prop;
        });

        Expression = processedExpression;
        IsExplicitFormula = isExplicitFormula;
        
        var parser = new JavaScriptParser();
        try 
        {
            Ast = parser.ParseScript(processedExpression);
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
