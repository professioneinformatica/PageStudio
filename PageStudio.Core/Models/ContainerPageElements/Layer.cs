using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Layer element that extends ContainerElement and implements ILayer interface
/// </summary>
public class Layer : GroupElement, ILayer
{
    /// <summary>
    /// Initializes a new instance of Layer
    /// </summary>
    /// <param name="page">The parent page containing this layer</param>
    /// <param name="name">Layer name</param>
    /// <param name="zOrder">Layer Z-index</param>
    public Layer(IEventPublisher eventPublisher, IPage page, string name = "Layer", int zOrder = 0) : base(eventPublisher, page, name)
    {
        ZIndex = zOrder;
    }
}