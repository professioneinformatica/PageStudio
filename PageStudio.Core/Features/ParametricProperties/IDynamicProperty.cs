namespace PageStudio.Core.Features.ParametricProperties;

public interface IDynamicProperty
{
    PropertyId Id { get; }
    void Invalidate();
    bool IsDirty { get; }
    object? GetValue();
    IEnumerable<PropertyId> Dependencies { get; }
}