using System.Collections.Generic;

namespace PageStudio.Core.Interfaces;

/// <summary>
/// Base interface for all elements that can be placed on a page
/// </summary>
public interface IPageElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    Guid Id { get; }
    
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
    /// Indicates whether the aspect ratio of the element should remain constant when resizing.
    /// </summary>
    public bool LockAspectRatio { get; set; }
    
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
    /// Whether this element can contain child elements
    /// </summary>
    bool CanContainChildren { get; }
    
    /// <summary>
    /// Collection of child elements
    /// </summary>
    IList<IPageElement> Childrens { get; }
    
    /// <summary>
    /// Renders the element using the provided graphics context
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    void Render(IGraphicsContext graphics);

    public int? HitTestHandle(double canvasX, double canvasY);
    
    /// <summary>
    /// Creates a deep copy of this element
    /// </summary>
    /// <returns>Cloned element</returns>
    IPageElement Clone();
    
    /// <summary>
    /// Adds an element to the layer
    /// </summary>
    /// <param name="element">Element to add</param>
    void AddChildren(IPageElement element);
    
    /// <summary>
    /// Removes an element from the layer
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    bool RemoveChildren(Guid elementId);
    
    /// <summary>
    /// Gets an element by its ID
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    IPageElement GetChildren(Guid elementId);
    
    /// <summary>
    /// Gets elements sorted by Z-order (lowest to highest)
    /// </summary>
    /// <returns>Elements sorted by Z-order</returns>
    IEnumerable<IPageElement> GetElementsByZOrder();
    
    /// <summary>
    /// Gets elements at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Elements at the specified position</returns>
    IEnumerable<IPageElement> GetElementsAtPosition(double x, double y);
    
    /// <summary>
    /// Clears all elements from the layer
    /// </summary>
    void ClearElements();

    /// <summary>
    /// Indicates if the element is currently selected (for UI rendering, e.g. border highlight)
    /// </summary>
    bool IsSelected { get; set; }
}