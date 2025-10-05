using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Base container element that can contain child elements
/// </summary>
public class GroupElement : PageElement
{
    /// <summary>
    /// Container elements can always contain children
    /// </summary>
    public override bool CanContainChildren => true;

    /// <summary>
    /// Collection of child elements
    /// </summary>
    public override IList<IPageElement> Childrens => _children;

    /// <summary>
    /// Initializes a new instance of ContainerElement
    /// </summary>
    /// <param name="name">Container name</param>
    public GroupElement(string name = "Container") : base(name)
    {
        
    }

    /// <summary>
    /// Container doesn't render anything itself, only its children
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    protected override void RenderSelf(IGraphicsContext graphics)
    {
        // Container doesn't render anything of itself
    }

    /// <summary>
    /// Creates a deep copy of this container element
    /// </summary>
    /// <returns>Cloned container element</returns>
    public override IPageElement Clone()
    {
        var clone = new GroupElement(Name)
        {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Rotation = Rotation,
            Opacity = Opacity,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            ZOrder = ZOrder,
            ModifiedAt = ModifiedAt
        };

        // Clone children
        foreach (var child in Childrens)
        {
            clone.Childrens.Add(child.Clone());
        }

        return clone;
    }
}