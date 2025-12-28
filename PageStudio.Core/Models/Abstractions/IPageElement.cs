using PageStudio.Core.Features.ParametricProperties;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Models.Abstractions;

/// <summary>
///     Base interface for all elements that can be placed on a page
/// </summary>
public interface IPageElement
{
    /// <summary>
    ///     Unique identifier for the element
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Represents the page on which the element is placed.
    /// </summary>
    IPage Page { get; }

    /// <summary>
    ///     Element name/title
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Indicates whether the element should be hidden from the document structure
    /// </summary>
    bool HideFromDocumentStructure { get; set; }

    // Parametric properties
    DynamicProperty<double> X { get; set; }
    DynamicProperty<double> Y { get; set; }
    DynamicProperty<double> Width { get; set; }
    DynamicProperty<double> Height { get; set; }
    DynamicProperty<double> Rotation { get; set; }
    DynamicProperty<double> Opacity { get; set; }
    DynamicProperty<bool> IsVisible { get; set; }
    DynamicProperty<bool> IsLocked { get; set; }

    /// <summary>
    ///     Indicates whether the aspect ratio of the element should remain constant when resizing.
    /// </summary>
    public bool LockAspectRatio { get; set; }

    /// <summary>
    ///     Represents the ratio of the width to the height of the element,
    ///     calculated as Width divided by Height.
    /// </summary>
    public double AspectRatio { get; }

    /// <summary>
    ///     Z-order of the element (higher values are on top)
    /// </summary>
    int ZIndex { get; set; }

    /// <summary>
    ///     Element creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    ///     Last modification timestamp
    /// </summary>
    DateTime ModifiedAt { get; set; }

    /// <summary>
    ///     Whether this element can contain child elements
    /// </summary>
    bool CanContainChildren { get; }

    public IPageElement? Parent { get; set; }

    /// <summary>
    ///     Collection of child elements
    /// </summary>
    IReadOnlyList<IPageElement> Children { get; }

    /// <summary>
    ///     Indicates if the element is currently selected (for UI rendering, e.g. border highlight)
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    ///     Retrieves the index of a specified child element within the current element's child collection.
    /// </summary>
    /// <param name="child">The child element whose index is to be determined.</param>
    /// <returns>The zero-based index of the specified child element, or -1 if the child element is not found.</returns>
    internal int GetChildIndex(IPageElement child);

    /// <summary>
    ///     Renders the element using the provided graphics context
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    void Render(IGraphicsContext graphics);

    public int? HitTestHandle(double canvasX, double canvasY);

    /// <summary>
    ///     Creates a deep copy of this element
    /// </summary>
    /// <returns>Cloned element</returns>
    IPageElement Clone();

    /// <summary>
    ///     Adds an element to the layer
    /// </summary>
    /// <param name="element">Element to add</param>
    void AddChild(IPageElement element);

    /// <summary>
    ///     Removes an element from the layer
    /// </summary>
    /// <param name="element">the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    bool RemoveChild(IPageElement element);

    /// <summary>
    ///     Removes an element from the layer
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    bool RemoveChild(Guid elementId);

    /// <summary>
    ///     Moves a child element to the specified index within the parent's children collection.
    /// </summary>
    /// <param name="element">The child element to move.</param>
    /// <param name="newIndex">The zero-based index in the children collection to move the element to.</param>
    void MoveChildToIndex(PageElement element, int newIndex);

    /// <summary>
    ///     Gets an element by its ID
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    IPageElement? GetChildren(Guid elementId);

    /// <summary>
    ///     Gets elements sorted by Z-order (lowest to highest)
    /// </summary>
    /// <returns>Elements sorted by Z-order</returns>
    IEnumerable<IPageElement> GetElementsByZOrder();

    /// <summary>
    ///     Gets elements at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Elements at the specified position</returns>
    IEnumerable<IPageElement> GetElementsAtPosition(double x, double y);

    /// <summary>
    ///     Clears all elements from the layer
    /// </summary>
    void ClearElements();
}
