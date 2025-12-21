using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Base container element that can contain child elements
/// </summary>
public class GroupElement : PageElement
{
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Container elements can always contain children
    /// </summary>
    public override bool CanContainChildren => true;

    /// <summary>
    /// Collection of child elements
    /// </summary>
    public override IReadOnlyList<IPageElement> Children => _children.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of ContainerElement
    /// </summary>
    /// <param name="page"></param>
    /// <param name="name">Container name</param>
    /// <param name="mediator"></param>
    public GroupElement(IEventPublisher eventPublisher, IPage page, string name = "Container") : base(eventPublisher, page, name)
    {
        _eventPublisher = eventPublisher;
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
        var clone = new GroupElement(_eventPublisher, this.Page, Name)
        {
            ZIndex = ZIndex,
            ModifiedAt = ModifiedAt,
            LockAspectRatio = LockAspectRatio
        };
        
        clone.X.Formula = X.Formula;
        clone.Y.Formula = Y.Formula;
        clone.Width.Formula = Width.Formula;
        clone.Height.Formula = Height.Formula;
        clone.Rotation.Formula = Rotation.Formula;
        clone.Opacity.Formula = Opacity.Formula;
        clone.IsVisible.Formula = IsVisible.Formula;
        clone.IsLocked.Formula = IsLocked.Formula;

        // Clone children
        foreach (var child in Children)
        {
            clone.AddChild(child.Clone());
        }

        return clone;
    }
}