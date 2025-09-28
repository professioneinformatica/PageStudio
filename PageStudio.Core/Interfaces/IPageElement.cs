namespace PageStudio.Core.Interfaces;

/// <summary>
/// Base interface for all elements that can be placed on a page
/// </summary>
public interface IPageElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Element name/title
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// X coordinate of the element
    /// </summary>
    double X { get; set; }
    
    /// <summary>
    /// Y coordinate of the element
    /// </summary>
    double Y { get; set; }
    
    /// <summary>
    /// Width of the element
    /// </summary>
    double Width { get; set; }
    
    /// <summary>
    /// Height of the element
    /// </summary>
    double Height { get; set; }
    
    /// <summary>
    /// Rotation angle in degrees
    /// </summary>
    double Rotation { get; set; }
    
    /// <summary>
    /// Element opacity (0.0 to 1.0)
    /// </summary>
    double Opacity { get; set; }
    
    /// <summary>
    /// Whether the element is visible
    /// </summary>
    bool IsVisible { get; set; }
    
    /// <summary>
    /// Whether the element is locked for editing
    /// </summary>
    bool IsLocked { get; set; }
    
    /// <summary>
    /// Z-order of the element (higher values are on top)
    /// </summary>
    int ZOrder { get; set; }
    
    /// <summary>
    /// Element creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    DateTime ModifiedAt { get; set; }
    
    /// <summary>
    /// Renders the element using the provided graphics context
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    void Render(IGraphicsContext graphics);
    
    /// <summary>
    /// Creates a deep copy of this element
    /// </summary>
    /// <returns>Cloned element</returns>
    IPageElement Clone();
}