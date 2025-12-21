namespace PageStudio.Core.Features.ParametricProperties;

public class ParametricEngine
{
    public EnginePool Pool { get; } = new();
    public SymbolTable Symbols { get; } = new();
    public DependencyGraph Graph { get; } = new();
    public EvaluationContext Context { get; }

    public ParametricEngine()
    {
        Context = new EvaluationContext(Pool, Symbols);
    }

    public DynamicProperty<T> CreateProperty<T>(Guid ownerId, string name, string initialExpression = "0")
    {
        var id = new PropertyId(ownerId, name);
        var formula = new JsFormula(initialExpression);
        var prop = new DynamicProperty<T>(id, formula, Context, Graph, Symbols);
        Symbols.RegisterProperty(prop);
        return prop;
    }

    public void RegisterElement(string symbolName, Guid id)
    {
        Symbols.RegisterElement(symbolName, id);
    }
}
