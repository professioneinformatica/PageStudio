namespace PageStudio.Core.Features.ParametricProperties;

public record SymbolEntry(Guid Id, string Name);

public class SymbolTable(ParametricEngine engine)
{
    public ParametricEngine Engine => engine;
    private readonly List<SymbolEntry> _symbols = new();
    private readonly Dictionary<PropertyId, IDynamicProperty> _registry = new();

    public void RegisterElement(string symbolName, Guid id)
    {
        _symbols.RemoveAll(s => s.Id == id);
        _symbols.Add(new SymbolEntry(id, symbolName));
    }

    public bool IsSymbolNameAvailable(string symbolName, Guid excludeId)
    {
        var entry = _symbols.FirstOrDefault(s => s.Name == symbolName);
        if (entry == null)
        {
            return true;
        }

        return entry.Id == excludeId;
    }

    public void RegisterProperty(IDynamicProperty property)
    {
        _registry[property.Id] = property;
    }

    public IDynamicProperty? Resolve(string symbolName, string propertyName)
    {
        if (symbolName.StartsWith("__id_"))
        {
            var guidStr = symbolName.Substring(5).Replace("_", "-");
            if (Guid.TryParse(guidStr, out var id))
            {
                return Resolve(new PropertyId(id, propertyName));
            }
        }

        var entry = _symbols.FirstOrDefault(s => s.Name == symbolName);
        if (entry != null)
        {
            return Resolve(new PropertyId(entry.Id, propertyName));
        }
        return null;
    }

    public IDynamicProperty? Resolve(PropertyId id)
    {
        _registry.TryGetValue(id, out var prop);
        return prop;
    }

    public string? GetSymbolName(Guid id) => _symbols.FirstOrDefault(s => s.Id == id)?.Name;
}
