using Jint;
using Jint.Native;

namespace PageStudio.Core.Features.ParametricProperties;

public class EvaluationContext(EnginePool pool, SymbolTable symbolTable)
{
    private readonly Stack<PropertyId> _evaluationStack = new();

    public object? Evaluate(IDynamicProperty property, JsFormula formula)
    {
        if (_evaluationStack.Contains(property.Id))
        {
            throw new InvalidOperationException($"Circular dependency detected at runtime for {property.Id}");
        }

        _evaluationStack.Push(property.Id);
        var engine = pool.Get();
        try
        {
            // Reset engine context by clearing globals if needed, 
            // but for simplicity we'll just inject what we need.
            // In a real scenario, we might want a cleaner way to reset.
            
            foreach (var dep in formula.Dependencies)
            {
                var targetProp = symbolTable.Resolve(dep.SymbolName, dep.PropertyName);
                if (targetProp != null)
                {
                    // Recursively get value
                    var val = targetProp.GetValue();
                    
                    // Inject into JS engine
                    // We need to ensure the object exists in JS
                    var obj = engine.GetValue(dep.SymbolName);
                    if (obj.IsUndefined())
                    {
                        engine.Evaluate($"{dep.SymbolName} = {{}}");
                    }
                    
                    var jsObj = engine.GetValue(dep.SymbolName).AsObject();
                    jsObj.Set(dep.PropertyName, JsValue.FromObject(engine, val));
                }
            }

            var result = engine.Evaluate(formula.Expression);
            return result.ToObject();
        }
        finally
        {
            pool.Return(engine);
            _evaluationStack.Pop();
        }
    }
}
