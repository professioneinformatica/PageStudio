using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.ContainerPageElements;
using PageStudio.Core.Models.Documents;
using SkiaSharp;

namespace PageStudio.Core.Models.Page;

/// <summary>
/// Implementation of IPage interface representing a page in a document
/// </summary>
public class Page : IPage
{
    private readonly IEventPublisher _eventPublisher;
    private readonly List<ILayer> _layers;
    private readonly ILayer _defaultLayer;

    /// <summary>
    /// Unique identifier for the page
    /// </summary>
    public Guid Id { get; }

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
    /// Collection of layers on this page
    /// </summary>
    public IList<ILayer> Layers => _layers;

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

    public bool IsActive { get; internal set; }

    public IDocument Document { get; init; }

    /// <summary>
    /// Initializes a new instance of Page
    /// </summary>
    /// <param name="document">Reference to the containing document</param>
    /// <param name="name">Page name</param>
    /// <param name="width">Page width in points</param>
    /// <param name="height">Page height in points</param>
    public Page(IEventPublisher eventPublisher, IDocument document, string name = "Page", double width = 595, double height = 842) // A4 size by default
    {
        _eventPublisher = eventPublisher;
        Document = document;
        Id = Guid.CreateVersion7();
        Name = name;
        Width = width;
        Height = height;
        Margins = new Margins(72); // 1 inch margins by default
        _layers = new List<ILayer>();
        Background = null;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        IsActive = false;

        // Create and add default layer
        _defaultLayer = new Layer(_eventPublisher, this, "Default Layer");
        _layers.Add(_defaultLayer);
    }

    /// <summary>
    /// Adds a layer to the page
    /// </summary>
    /// <param name="layer">Layer to add</param>
    public void AddLayer(ILayer layer)
    {
        if (layer == null)
            throw new ArgumentNullException(nameof(layer));

        // Check if layer with same ID already exists
        if (_layers.Any(l => l.Id == layer.Id))
            throw new InvalidOperationException($"Layer with ID '{layer.Id}' already exists on this page.");

        _layers.Add(layer);

        UpdateModifiedTime();
    }

    /// <summary>
    /// Removes a layer from the page
    /// </summary>
    /// <param name="layerId">ID of the layer to remove</param>
    /// <returns>True if layer was removed, false otherwise</returns>
    public bool RemoveLayer(Guid layerId)
    {
        if (layerId == Guid.Empty)
            return false;

        // Cannot remove the default layer
        if (_defaultLayer.Id == layerId)
            throw new InvalidOperationException("Cannot remove the default layer.");

        var layer = _layers.FirstOrDefault(l => l.Id == layerId);
        if (layer == null)
            return false;

        var removed = _layers.Remove(layer);
        if (removed)
            UpdateModifiedTime();

        return removed;
    }

    /// <summary>
    /// Gets a layer by its ID
    /// </summary>
    /// <param name="layerId">Layer ID</param>
    /// <returns>The layer if found, null otherwise</returns>
    public ILayer? GetLayer(Guid layerId)
    {
        if (layerId == Guid.Empty)
            return null;

        return _layers.FirstOrDefault(l => l.Id == layerId);
    }

    /// <summary>
    /// Gets layers sorted by Z-index (lowest to highest)
    /// </summary>
    /// <returns>Layers sorted by Z-index</returns>
    public IEnumerable<ILayer> GetLayersByZIndex()
    {
        return _layers.OrderBy(l => l.ZIndex);
    }

    /// <summary>
    /// Gets the default layer for the page
    /// </summary>
    /// <returns>The default layer</returns>
    public ILayer GetDefaultLayer()
    {
        return _defaultLayer;
    }

    /// <summary>
    /// Adds an element to the default layer
    /// </summary>
    /// <param name="element">Element to add</param>
    public IPageElement AddElement(IPageElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        _defaultLayer.AddChild(element);
        UpdateModifiedTime();
        return element;
    }

    /// <summary>
    /// Adds an element to a specific layer
    /// </summary>
    /// <param name="element">Element to add</param>
    /// <param name="layerId">ID of the target layer</param>
    public void AddElementToLayer(IPageElement element, Guid layerId)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        var layer = GetLayer(layerId);
        if (layer == null)
            throw new ArgumentException($"Layer with ID '{layerId}' not found.");

        layer.AddChild(element);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Removes an element from any layer on the page
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    public bool RemoveElement(Guid elementId)
    {
        if (elementId == Guid.Empty)
            return false;

        foreach (var layer in _layers)
        {
            if (layer.RemoveChild(elementId))
            {
                UpdateModifiedTime();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets an element by its ID from any layer
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    public IPageElement? GetElement(Guid elementId)
    {
        if (elementId == Guid.Empty)
            return null;

        foreach (var layer in _layers)
        {
            var element = layer.GetChildren(elementId);
            if (element != null)
                return element;
        }

        return null;
    }

    /// <summary>
    /// Gets all elements from all layers sorted by layer Z-index then element Z-order
    /// </summary>
    /// <returns>All elements sorted by rendering order</returns>
    public IEnumerable<IPageElement> GetAllElementsByRenderOrder()
    {
        var result = new List<IPageElement>();

        // Process layers by their Z-index (lowest to highest)
        foreach (var layer in GetLayersByZIndex())
        {
            if (layer.IsVisible.Value)
            {
                // Add elements from this layer sorted by their Z-order
                result.AddRange(layer.GetElementsByZOrder());
            }
        }

        return result;
    }

    /// <summary>
    /// Gets elements sorted by Z-order (for backward compatibility)
    /// </summary>
    /// <returns>Elements sorted by Z-order</returns>
    public IEnumerable<IPageElement> GetElementsByZOrder()
    {
        return GetAllElementsByRenderOrder();
    }

    /// <summary>
    /// Gets elements at a specific position from all layers
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Elements at the specified position</returns>
    public IEnumerable<IPageElement> GetElementsAtPosition(double x, double y)
    {
        var result = new List<IPageElement>();

        foreach (var layer in _layers)
        {
            if (layer.IsVisible.Value)
            {
                result.AddRange(layer.GetElementsAtPosition(x, y));
            }
        }

        return result;
    }

    /// <summary>
    /// Clears all elements from all layers
    /// </summary>
    public void ClearElements()
    {
        var hasElements = false;
        foreach (var layer in _layers)
        {
            if (layer.Children.Count > 0)
            {
                layer.ClearElements();
                hasElements = true;
            }
        }

        if (hasElements)
            UpdateModifiedTime();
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp
    /// </summary>
    private void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Renderizza la pagina su un contesto grafico
    /// </summary>
    /// <param name="graphics">Contesto grafico</param>
    public void Render(
        IGraphicsContext graphics)
    {
        graphics.Save();
        try
        {
            var strokeWidth = Math.Max(1.0f / this.Document.CanvasInteractor.ZoomManager.Level, 0.5f);
            // graphics.Translate(xOffset, yOffset);

            // Sfondo pagina
            using var backgroundPaint = new SKPaint();
            backgroundPaint.Color = SKColors.White;
            backgroundPaint.Style = SKPaintStyle.Fill;
            graphics.DrawRect(new SKRect(0, 0, (float)Width, (float)Height), backgroundPaint);

            // Bordo pagina
            using var borderPaint = new SKPaint();
            borderPaint.Color = SKColors.LightGray;
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = strokeWidth;
            graphics.DrawRect(new SKRect(0, 0, (float)Width, (float)Height), borderPaint);

            // Elementi
            var elements = GetAllElementsByRenderOrder();
            foreach (var element in elements)
            {
                element.Render(graphics);
            }
        }
        finally
        {
            graphics.Restore();
        }
    }
}