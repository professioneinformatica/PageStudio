using System.Collections.Generic;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models;

/// <summary>
/// Implementation of IPage interface representing a page in a document
/// </summary>
public class Page : IPage
{
    private readonly List<IPageElement> _elements;

    /// <summary>
    /// Unique identifier for the page
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Page name/title
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Page width in points
    /// </summary>
    public double Width { get; set; }
    
    /// <summary>
    /// Page height in points
    /// </summary>
    public double Height { get; set; }
    
    /// <summary>
    /// Page margins
    /// </summary>
    public IMargins Margins { get; set; }
    
    /// <summary>
    /// Collection of elements on this page
    /// </summary>
    public IList<IPageElement> Elements => _elements;
    
    /// <summary>
    /// Page background color or pattern
    /// </summary>
    public object? Background { get; set; }
    
    /// <summary>
    /// Page creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of Page
    /// </summary>
    /// <param name="name">Page name</param>
    /// <param name="width">Page width in points</param>
    /// <param name="height">Page height in points</param>
    public Page(string name = "Page", double width = 595, double height = 842) // A4 size by default
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Width = width;
        Height = height;
        Margins = new Margins(72); // 1 inch margins by default
        _elements = new List<IPageElement>();
        Background = null;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an element to the page
    /// </summary>
    /// <param name="element">Element to add</param>
    public void AddElement(IPageElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        // Check if element with same ID already exists
        if (_elements.Any(e => e.Id == element.Id))
            throw new InvalidOperationException($"Element with ID '{element.Id}' already exists on this page.");

        _elements.Add(element);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Removes an element from the page
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    public bool RemoveElement(string elementId)
    {
        if (string.IsNullOrEmpty(elementId))
            return false;

        var element = _elements.FirstOrDefault(e => e.Id == elementId);
        if (element == null)
            return false;

        var removed = _elements.Remove(element);
        if (removed)
            UpdateModifiedTime();

        return removed;
    }

    /// <summary>
    /// Gets an element by its ID
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    public IPageElement? GetElement(string elementId)
    {
        if (string.IsNullOrEmpty(elementId))
            return null;

        return _elements.FirstOrDefault(e => e.Id == elementId);
    }

    /// <summary>
    /// Gets elements sorted by Z-order (lowest to highest)
    /// </summary>
    /// <returns>Elements sorted by Z-order</returns>
    public IEnumerable<IPageElement> GetElementsByZOrder()
    {
        return _elements.OrderBy(e => e.ZOrder);
    }

    /// <summary>
    /// Gets elements at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Elements at the specified position</returns>
    public IEnumerable<IPageElement> GetElementsAtPosition(double x, double y)
    {
        return _elements.Where(e => e.IsVisible && 
                                   x >= e.X && x <= e.X + e.Width &&
                                   y >= e.Y && y <= e.Y + e.Height);
    }

    /// <summary>
    /// Clears all elements from the page
    /// </summary>
    public void ClearElements()
    {
        if (_elements.Count > 0)
        {
            _elements.Clear();
            UpdateModifiedTime();
        }
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp
    /// </summary>
    private void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}