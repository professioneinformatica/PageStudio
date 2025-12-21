namespace PageStudio.Core.Features.ParametricProperties;

public class DependencyGraph
{
    private readonly Dictionary<PropertyId, HashSet<PropertyId>> _dependents = new();

    public void UpdateDependencies(PropertyId propertyId, IEnumerable<PropertyId> dependencies)
    {
        // Remove old dependencies
        foreach (var pair in _dependents)
        {
            pair.Value.Remove(propertyId);
        }

        // Add new ones
        foreach (var depId in dependencies)
        {
            if (!_dependents.ContainsKey(depId))
                _dependents[depId] = new HashSet<PropertyId>();
            
            _dependents[depId].Add(propertyId);
        }
    }

    public void Invalidate(PropertyId propertyId, SymbolTable symbolTable)
    {
        if (_dependents.TryGetValue(propertyId, out var dependents))
        {
            foreach (var dependentId in dependents)
            {
                var prop = symbolTable.Resolve(dependentId);
                if (prop is { IsDirty: false })
                {
                    prop.Invalidate();
                    Invalidate(dependentId, symbolTable);
                }
            }
        }
    }

    public bool WouldCreateCycle(PropertyId propertyId, IEnumerable<PropertyId> newDependencies)
    {
        var visited = new HashSet<PropertyId>();

        foreach (var dep in newDependencies)
        {
            if (IsReachable(propertyId, dep, visited))
                return true;
        }

        return false;
    }

    private bool IsReachable(PropertyId source, PropertyId target, HashSet<PropertyId> visited)
    {
        if (source == target) return true;
        if (!visited.Add(source)) return false;

        if (_dependents.TryGetValue(source, out var dependents))
        {
            foreach (var dep in dependents)
            {
                if (IsReachable(dep, target, visited))
                    return true;
            }
        }

        return false;
    }
}
