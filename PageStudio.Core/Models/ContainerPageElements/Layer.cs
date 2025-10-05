using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Layer element that extends ContainerElement and implements ILayer interface
/// </summary>
public class Layer : GroupElement, ILayer
{
    
    /// <summary>
    /// Initializes a new instance of Layer
    /// </summary>
    /// <param name="name">Layer name</param>
    /// <param name="zOrder">Layer Z-index</param>
    public Layer(string name = "Layer", int zOrder = 0) : base(name)
    {
        ZOrder = zOrder;
    }
    
}