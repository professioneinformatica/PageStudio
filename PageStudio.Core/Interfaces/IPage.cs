using System.Collections.Generic;
using Mediator;

namespace PageStudio.Core.Interfaces;

/// <summary>
/// Represents a page in a document
/// </summary>
public interface IPage
{
    
    IMediator InternalMediator { get; }
    
    /// <summary>
    /// Unique identifier for the page
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The document associated with the page.
    /// </summary>
    IDocument Document { get; }

    /// <summary>
    /// Page name/title
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Page width in points
    /// </summary>
    double Width { get; set; }
    
    /// <summary>
    /// Page height in points
    /// </summary>
    double Height { get; set; }
    
    /// <summary>
    /// Page margins
    /// </summary>
    IMargins Margins { get; set; }
    
    /// <summary>
    /// Collection of layers on this page
    /// </summary>
    IList<ILayer> Layers { get; }
    
    /// <summary>
    /// Page background color or pattern
    /// </summary>
    object? Background { get; set; }
    
    /// <summary>
    /// Page creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    DateTime ModifiedAt { get; set; }
    
    /// <summary>
    /// Indica se la pagina è attiva
    /// </summary>
    public bool IsActive { get; }
    
    /// <summary>
    /// Adds a layer to the page
    /// </summary>
    /// <param name="layer">Layer to add</param>
    void AddLayer(ILayer layer);
    
    /// <summary>
    /// Removes a layer from the page
    /// </summary>
    /// <param name="layerId">ID of the layer to remove</param>
    /// <returns>True if layer was removed, false otherwise</returns>
    bool RemoveLayer(Guid layerId);
    
    /// <summary>
    /// Gets a layer by its ID
    /// </summary>
    /// <param name="layerId">Layer ID</param>
    /// <returns>The layer if found, null otherwise</returns>
    ILayer? GetLayer(Guid layerId);
    
    /// <summary>
    /// Gets layers sorted by Z-index (lowest to highest)
    /// </summary>
    /// <returns>Layers sorted by Z-index</returns>
    IEnumerable<ILayer> GetLayersByZIndex();
    
    /// <summary>
    /// Gets the default layer for the page
    /// </summary>
    /// <returns>The default layer</returns>
    ILayer GetDefaultLayer();

    /// <summary>
    /// Adds an element to the default layer
    /// </summary>
    /// <param name="element">Element to add</param>
    IPageElement AddElement(IPageElement element);
    
    /// <summary>
    /// Adds an element to a specific layer
    /// </summary>
    /// <param name="element">Element to add</param>
    /// <param name="layerId">ID of the target layer</param>
    void AddElementToLayer(IPageElement element, Guid layerId);
    
    /// <summary>
    /// Removes an element from any layer on the page
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    bool RemoveElement(Guid elementId);
    
    /// <summary>
    /// Gets an element by its ID from any layer
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    IPageElement? GetElement(Guid elementId);
    
    /// <summary>
    /// Gets all elements from all layers sorted by layer Z-index then element Z-order
    /// </summary>
    /// <returns>All elements sorted by rendering order</returns>
    IEnumerable<IPageElement> GetAllElementsByRenderOrder();
}