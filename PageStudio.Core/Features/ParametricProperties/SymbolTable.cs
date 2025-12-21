namespace PageStudio.Core.Features.ParametricProperties;

public class SymbolTable
{
    private readonly Dictionary<string, Guid> _symbolToId = new();
    private readonly Dictionary<Guid, string> _idToSymbol = new();
    private readonly Dictionary<PropertyId, IDynamicProperty> _registry = new();

    public void RegisterElement(string symbolName, Guid id)
    {
        _symbolToId[symbolName] = id;
        _idToSymbol[id] = symbolName;
    }

    public void RegisterProperty(IDynamicProperty property)
    {
        _registry[property.Id] = property;
    }

    public IDynamicProperty? Resolve(string symbolName, string propertyName)
    {
        if (_symbolToId.TryGetValue(symbolName, out var ownerId))
        {
            return Resolve(new PropertyId(ownerId, propertyName));
        }
        return null;
    }

    public IDynamicProperty? Resolve(PropertyId id)
    {
        _registry.TryGetValue(id, out var prop);
        return prop;
    }

    public string? GetSymbolName(Guid id) => _idToSymbol.GetValueOrDefault(id);
}
