using System.Collections.Generic;

namespace PageStudio.Core.Interfaces;

/// <summary>
/// Represents a document in the PageStudio application
/// </summary>
public interface IDocument
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Document name/title
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Collection of pages in the document
    /// </summary>
    IList<IPage> Pages { get; }
    
    /// <summary>
    /// Document metadata
    /// </summary>
    Dictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Document creation timestamp
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Adds a new page to the document
    /// </summary>
    /// <param name="page">Page to add</param>
    /// <param name="insertIndex">The index where insert page. Null to add the page at the end</param>
    void AddPage(IPage page, int? insertIndex);
    
    /// <summary>
    /// Removes a page from the document
    /// </summary>
    /// <param name="pageId">ID of the page to remove</param>
    /// <returns>True if page was removed, false otherwise</returns>
    bool RemovePage(Guid pageId);
    
    /// <summary>
    /// Gets a page by its ID
    /// </summary>
    /// <param name="pageId">Page ID</param>
    /// <returns>The page if found, null otherwise</returns>
    IPage GetPage(Guid pageId);
}