namespace PageStudio.Core.Features.ParametricProperties;

public record PropertyId(Guid OwnerId, string PropertyName);

public class DynamicProperty<T> : IDynamicProperty
{
    private readonly EvaluationContext _context;
    private readonly DependencyGraph _graph;
    private readonly SymbolTable _symbolTable;
    
    public PropertyId Id { get; }
    private JsFormula _formula;
    private T? _cachedValue;
    public bool IsDirty { get; private set; } = true;

    public DynamicProperty(
        PropertyId id, 
        JsFormula formula, 
        EvaluationContext context, 
        DependencyGraph graph, 
        SymbolTable symbolTable)
    {
        Id = id;
        _context = context;
        _graph = graph;
        _symbolTable = symbolTable;

        // Validate formula on creation
        try
        {
            _context.Validate(Id, formula);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid JS formula in constructor: {formula.Expression}", ex);
        }

        _formula = formula;
        UpdateGraph();
    }

    public JsFormula Formula
    {
        get => _formula;
        set
        {
            var newDeps = ResolveDependencies(value);
            if (_graph.WouldCreateCycle(Id, newDeps))
            {
                throw new InvalidOperationException("Assigning this formula would create a circular dependency.");
            }

            // Validate formula before assignment
            try
            {
                _context.Validate(Id, value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid JS formula execution: {value.Expression}", ex);
            }

            _formula = value;
            UpdateGraph();
            Invalidate();
        }
    }

    public T? Value
    {
        get => GetValueT();
        set => Formula = JsFormula.FromConstant(value!);
    }

    public string FormulaExpression
    {
        get => _formula.Expression;
        set => Formula = new JsFormula(value);
    }

    public bool IsConstant => _formula.Dependencies.Count == 0 && !_formula.IsExplicitFormula;

    public bool IsExplicitFormula => _formula.IsExplicitFormula;

    public object? GetValue() => GetValueT();

    public Action<T?>? OnValueChanged { get; set; }

    private T? GetValueT()
    {
        if (IsDirty)
        {
            var result = _context.Evaluate(this, _formula);
            var oldValue = _cachedValue;
            _cachedValue = ConvertValue(result);
            IsDirty = false;
            
            if (!EqualityComparer<T>.Default.Equals(oldValue, _cachedValue))
            {
                OnValueChanged?.Invoke(_cachedValue);
            }
        }
        return _cachedValue;
    }

    private static T? ConvertValue(object? value)
    {
        if (value == null) return default;
        if (value is T typedValue) return typedValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public void Invalidate()
    {
        if (!IsDirty)
        {
            IsDirty = true;
            _graph.Invalidate(Id, _symbolTable);
        }
    }

    public IEnumerable<PropertyId> Dependencies => ResolveDependencies(_formula);

    private void UpdateGraph()
    {
        _graph.UpdateDependencies(Id, Dependencies);
    }

    private List<PropertyId> ResolveDependencies(JsFormula formula)
    {
        var resolved = new List<PropertyId>();
        foreach (var dep in formula.Dependencies)
        {
            var prop = _symbolTable.Resolve(dep.SymbolName, dep.PropertyName);
            if (prop != null)
            {
                resolved.Add(prop.Id);
            }
        }
        return resolved;
    }
}
