using Ardalis.GuardClauses;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.Page;
using PageStudio.Core.Services;
using SkiaSharp;

namespace PageStudio.Core.Models.Documents;

/// <summary>
/// Unit of measure enumeration for document dimensions
/// </summary>
public enum UnitOfMeasure
{
    Centimeters,
    Inches
}

public record ZIndexChangedMessage(IPageElement element, int oldZIndex) : IEvent;

/// <summary>
/// Implementation of IDocument interface representing a document in the PageStudio application
/// </summary>
public class Document : IDocument
{
    private readonly IEventPublisher _eventPublisher;
    private readonly List<IPage> _pages;

    public CanvasDocumentInteractor CanvasInteractor { get; set; }

    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public Guid Id { get; }

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
    /// Default page format for new pages added to this document
    /// </summary>
    public PageFormat DefaultPageFormat { get; set; }

    /// <summary>
    /// Document DPI (Dots Per Inch) setting
    /// </summary>
    public int Dpi { get; set; }

    /// <summary>
    /// Unit of measure for document dimensions
    /// </summary>
    public UnitOfMeasure UnitOfMeasure { get; set; }

    /// <inheritdoc cref="Surface" />
    public SKSurface Surface { get; set; }

    /// <summary>
    /// Initializes a new instance of Document
    /// </summary>
    /// <param name="eventPublisher"></param>
    /// <param name="name">Document name</param>
    public Document(IEventPublisher eventPublisher, string name = "New Document")
    {
        _eventPublisher = eventPublisher;
        Id = Guid.CreateVersion7();
        Name = name;
        _pages = new List<IPage>();
        Metadata = new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        this.CanvasInteractor = new(eventPublisher);
        
        // Initialize default page format (A4 Portrait)
        DefaultPageFormat = PageFormat.Create(StandardPageFormat.A4, PageOrientation.Portrait);

        // Initialize DPI and unit of measure with defaults
        Dpi = 72; // Standard 72 DPI
        UnitOfMeasure = UnitOfMeasure.Centimeters;

        // Initialize with basic metadata
        Metadata["Creator"] = "PageStudio";
        Metadata["Version"] = "1.0";
        Metadata["DPI"] = Dpi;
        Metadata["UnitOfMeasure"] = UnitOfMeasure.ToString();
    }

    /// <summary>
    /// Adds a new page to the document
    /// </summary>
    /// <param name="page">Page to add</param>
    /// <param name="insertIndex">The index where you want to add the page. If not specified is appended to the end of the document.</param>
    public async Task AddPage(IPage page, int? insertIndex)
    {
        ArgumentNullException.ThrowIfNull(page);

        // Check if page with same ID already exists
        if (_pages.Any(p => p.Id == page.Id))
            throw new InvalidOperationException($"Page with ID '{page.Id}' already exists in this document.");

        if (!insertIndex.HasValue)
        {
            insertIndex = _pages.Count;
        }

        _pages.Insert(insertIndex.Value, page);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Removes a page from the document
    /// </summary>
    /// <param name="pageId">ID of the page to remove</param>
    /// <returns>True if page was removed, false otherwise</returns>
    public async Task<bool> RemovePage(Guid pageId)
    {
        var page = _pages.FirstOrDefault(p => p.Id == pageId);
        Guard.Against.Null(page);

        var removed = _pages.Remove(page);
        if (removed)
        {
            UpdateModifiedTime();
        }

        return await Task.FromResult(removed);
    }

    /// <summary>
    /// Gets a page by its ID
    /// </summary>
    /// <param name="pageId">Page ID</param>
    /// <returns>The page if found, null otherwise</returns>
    public IPage GetPage(Guid pageId)
    {
        var ret = _pages.FirstOrDefault(p => p.Id == pageId);
        Guard.Against.Null(ret);
        return ret;
    }

    /// <summary>
    /// Gets a page by its index
    /// </summary>
    /// <param name="index">Page index (0-based)</param>
    /// <returns>The page if found, null otherwise</returns>
    public IPage? GetPageByIndex(int index)
    {
        Guard.Against.OutOfRange(index, nameof(index), 0, _pages.Count);

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
    public async Task<bool> MovePage(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _pages.Count ||
            toIndex < 0 || toIndex >= _pages.Count ||
            fromIndex == toIndex)
            return false;

        var page = _pages[fromIndex];
        _pages.RemoveAt(fromIndex);
        _pages.Insert(toIndex, page);
        UpdateModifiedTime();
        return await Task.FromResult(true);
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

    public void Render(IGraphicsContext graphics)
    {
        graphics.Canvas.Clear(SKColors.White);
        graphics.Canvas.Save();
        graphics.Canvas.Translate(this.CanvasInteractor.PanOffsetX, this.CanvasInteractor.PanOffsetY);
        graphics.Canvas.Scale(this.CanvasInteractor.ZoomManager.Level);

        // Create graphics context

        // Render pages based on layout mode
        if (this.CanvasInteractor.CurrentLayoutMode == LayoutMode.Vertical)
        {
            RenderPagesVertically(graphics);
        }
        else
        {
            RenderPagesSideBySide(graphics);
        }

        // Restore canvas state after zoom transformation
        graphics.Canvas.Restore();
        
    }

    private void RenderPagesSideBySide(IGraphicsContext context)
    {
        var pages = this.Pages.ToList();
        var maxPageHeight = 0f;

        for (var i = 0; i < pages.Count; i++)
        {
            var page = pages[i];

            // If this is an even index (0, 2, 4...), start a new row
            float xOffset;
            float yOffset = 0;
            if (i % 2 == 0)
            {
                xOffset = 0f;
                if (i > 0)
                {
                    yOffset += maxPageHeight + 20; // Move to the next row with spacing
                }

                maxPageHeight = 0f;
            }
            else
            {
                // Odd index (1, 3, 5...), place to the right of the previous page
                var previousPage = pages[i - 1];
                xOffset = (float)previousPage.Width + 20; // Add spacing between side-by-side pages
            }

            context.Translate(xOffset, yOffset);
            page.Render(context);

            // Track the maximum height in this row
            maxPageHeight = Math.Max(maxPageHeight, (float)page.Height);
        }
    }

    private void RenderPagesVertically(IGraphicsContext context)
    {
        var yOffset = 0f;
        foreach (var page in this.Pages)
        {
            page.Render(context);
            yOffset += (float)page.Height + 20; // Add some spacing between pages
        }
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
    public void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    public async Task<List<IPage>> AddPages(PageFormat pageFormat, int numberOfPagesToAdd, int? startPage = null)
    {
        if (!startPage.HasValue)
        {
            startPage = Pages.Count();
        }

        var emptyDocument = Pages.Count == 0;
        var addedPages = new List<IPage>();

        for (int i = 0; i < numberOfPagesToAdd; i++)
        {
            var page = new Page.Page(_eventPublisher, this)
            {
                Width = pageFormat.ActualWidth,
                Height = pageFormat.ActualHeight,
                Name = $"Page {Pages.Count() + 1}",
                IsActive = emptyDocument // set the IsActive flag to true if this is the first page added to the document
            };
            addedPages.Add(page);
            await this.AddPage(page, startPage.Value + i);
        }

        return addedPages;
    }

    /// <summary>
    /// Imposta la pagina attiva tramite il suo ID. Solo una pagina per volta può essere attiva.
    /// </summary>
    /// <param name="pageId">ID della pagina da attivare</param>
    public async Task SetActivePage(Guid pageId)
    {
        foreach (var page in _pages)
        {
            var concretePage = (Page.Page)page;
            concretePage.IsActive = page.Id == pageId;
        }

        await Task.CompletedTask;
    }
}