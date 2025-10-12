using System.Collections.Generic;
using PageStudio.Core.Models;

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
    Task AddPage(IPage page, int? insertIndex);

    /// <summary>
    /// Adds multiple pages to the document using the specified page format.
    /// </summary>
    /// <param name="pageFormat">The format to use for the new pages.</param>
    /// <param name="numberOfPagesToAdd">The number of pages to add to the document.</param>
    /// <param name="startPage">The optional index where the pages should start being inserted. If null, pages will be added to the end of the document.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of the pages added to the document.</returns>
    Task<List<IPage>> AddPages(PageFormat pageFormat, int numberOfPagesToAdd, int? startPage = null);

    /// <summary>
    /// Removes a page from the document
    /// </summary>
    /// <param name="pageId">ID of the page to remove</param>
    /// <returns>True if page was removed, false otherwise</returns>
    Task<bool> RemovePage(Guid pageId);

    /// <summary>
    /// Moves a page to a different position within the document
    /// </summary>
    /// <param name="fromIndex">The current index of the page to be moved</param>
    /// <param name="toIndex">The target index to where the page should be moved</param>
    /// <returns>True if the page was moved successfully, false otherwise</returns>
    Task<bool> MovePage(int fromIndex, int toIndex);

    /// <summary>
    /// Gets a page by its ID
    /// </summary>
    /// <param name="pageId">Page ID</param>
    /// <returns>The page if found, null otherwise</returns>
    IPage GetPage(Guid pageId);

    /// <summary>
    /// Sets the active page in the document
    /// </summary>
    /// <param name="pageId">The ID of the page to set as active</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SetActivePage(Guid pageId);
}