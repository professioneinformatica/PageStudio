using System.Collections.Generic;

namespace PageStudio.Core.Interfaces;

/// <summary>
/// Represents a page in a document
/// </summary>
public interface IPage
{
    /// <summary>
    /// Unique identifier for the page
    /// </summary>
    string Id { get; }
    
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
    /// Collection of elements on this page
    /// </summary>
    IList<IPageElement> Elements { get; }
    
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
    /// Adds an element to the page
    /// </summary>
    /// <param name="element">Element to add</param>
    void AddElement(IPageElement element);
    
    /// <summary>
    /// Removes an element from the page
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    bool RemoveElement(string elementId);
    
    /// <summary>
    /// Gets an element by its ID
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    IPageElement? GetElement(string elementId);
}