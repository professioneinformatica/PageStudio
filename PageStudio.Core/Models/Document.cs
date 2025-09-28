using System.Collections.Generic;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models;

/// <summary>
/// Implementation of IDocument interface representing a document in the PageStudio application
/// </summary>
public class Document : IDocument
{
    private readonly List<IPage> _pages;

    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Document name/title
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Collection of pages in the document
    /// </summary>
    public IList<IPage> Pages => _pages;
    
    /// <summary>
    /// Document metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Document creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of Document
    /// </summary>
    /// <param name="name">Document name</param>
    public Document(string name = "New Document")
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        _pages = new List<IPage>();
        Metadata = new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        
        // Initialize with basic metadata
        Metadata["Creator"] = "PageStudio";
        Metadata["Version"] = "1.0";
    }

    /// <summary>
    /// Adds a new page to the document
    /// </summary>
    /// <param name="page">Page to add</param>
    public void AddPage(IPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        // Check if page with same ID already exists
        if (_pages.Any(p => p.Id == page.Id))
            throw new InvalidOperationException($"Page with ID '{page.Id}' already exists in this document.");

        _pages.Add(page);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Removes a page from the document
    /// </summary>
    /// <param name="pageId">ID of the page to remove</param>
    /// <returns>True if page was removed, false otherwise</returns>
    public bool RemovePage(string pageId)
    {
        if (string.IsNullOrEmpty(pageId))
            return false;

        var page = _pages.FirstOrDefault(p => p.Id == pageId);
        if (page == null)
            return false;

        var removed = _pages.Remove(page);
        if (removed)
            UpdateModifiedTime();

        return removed;
    }

    /// <summary>
    /// Gets a page by its ID
    /// </summary>
    /// <param name="pageId">Page ID</param>
    /// <returns>The page if found, null otherwise</returns>
    public IPage? GetPage(string pageId)
    {
        if (string.IsNullOrEmpty(pageId))
            return null;

        return _pages.FirstOrDefault(p => p.Id == pageId);
    }

    /// <summary>
    /// Gets a page by its index
    /// </summary>
    /// <param name="index">Page index (0-based)</param>
    /// <returns>The page if found, null otherwise</returns>
    public IPage? GetPageByIndex(int index)
    {
        if (index < 0 || index >= _pages.Count)
            return null;

        return _pages[index];
    }

    /// <summary>
    /// Inserts a page at a specific index
    /// </summary>
    /// <param name="index">Index to insert at</param>
    /// <param name="page">Page to insert</param>
    public void InsertPage(int index, IPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        if (index < 0 || index > _pages.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        // Check if page with same ID already exists
        if (_pages.Any(p => p.Id == page.Id))
            throw new InvalidOperationException($"Page with ID '{page.Id}' already exists in this document.");

        _pages.Insert(index, page);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Moves a page to a different position
    /// </summary>
    /// <param name="fromIndex">Current index of the page</param>
    /// <param name="toIndex">New index for the page</param>
    /// <returns>True if page was moved successfully</returns>
    public bool MovePage(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _pages.Count ||
            toIndex < 0 || toIndex >= _pages.Count ||
            fromIndex == toIndex)
            return false;

        var page = _pages[fromIndex];
        _pages.RemoveAt(fromIndex);
        _pages.Insert(toIndex, page);
        UpdateModifiedTime();

        return true;
    }

    /// <summary>
    /// Clears all pages from the document
    /// </summary>
    public void ClearPages()
    {
        if (_pages.Count > 0)
        {
            _pages.Clear();
            UpdateModifiedTime();
        }
    }

    /// <summary>
    /// Gets the total number of elements across all pages
    /// </summary>
    /// <returns>Total element count</returns>
    public int GetTotalElementCount()
    {
        return _pages.Sum(p => p.Elements.Count);
    }

    /// <summary>
    /// Sets metadata value
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    public void SetMetadata(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        Metadata[key] = value;
        UpdateModifiedTime();
    }

    /// <summary>
    /// Gets metadata value
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <returns>Metadata value or null if not found</returns>
    public object? GetMetadata(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        return Metadata.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp
    /// </summary>
    private void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}