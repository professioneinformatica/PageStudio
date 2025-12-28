using PageStudio.Core.Models.Documents;
namespace PageStudio.Core.Features.ParametricProperties;

public class ParametricEngine
{
    public EnginePool Pool { get; } = new();
    public SymbolTable Symbols { get; }
    public DependencyGraph Graph { get; } = new();
    public EvaluationContext Context { get; }
    public FormulaTranslator Translator { get; }

    public ParametricEngine(IDocument document)
    {
        Symbols = new SymbolTable(this);
        Context = new EvaluationContext(Pool, Symbols);
        Translator = new FormulaTranslator(document);
    }

    public DynamicProperty<T> CreateProperty<T>(Guid ownerId, string name, T initialValue)
    {
        var id = new PropertyId(ownerId, name);
        var formula = JsFormula.FromConstant(initialValue!);
        var prop = new DynamicProperty<T>(id, formula, Context, Graph, Symbols);
        Symbols.RegisterProperty(prop);
        return prop;
    }

    public DynamicProperty<T> CreateProperty<T>(Guid ownerId, string name, string formulaExpression = "0")
    {
        var id = new PropertyId(ownerId, name);
        var formula = new JsFormula(formulaExpression);
        var prop = new DynamicProperty<T>(id, formula, Context, Graph, Symbols);
        Symbols.RegisterProperty(prop);
        return prop;
    }

    public void RegisterElement(string symbolName, Guid id)
    {
        Symbols.RegisterElement(symbolName, id);
    }
}
