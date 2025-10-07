using Ardalis.GuardClauses;
using PageStudio.Core.Interfaces;
using SkiaSharp;

namespace PageStudio.Core.Models.Abstractions;

/// <summary>
/// Abstract base implementation of IPageElement interface for all page elements
/// </summary>
public abstract class PageElement : IPageElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Element name/title
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// X coordinate of the element
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate of the element
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width of the element
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Height of the element
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Rotation angle in degrees
    /// </summary>
    public double Rotation { get; set; }

    /// <summary>
    /// Element opacity (0.0 to 1.0)
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Whether the element is visible
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Whether the element is locked for editing
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Z-order of the element (higher values are on top)
    /// </summary>
    public int ZOrder { get; set; }

    /// <summary>
    /// Element creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Whether this element can contain child elements
    /// </summary>
    public virtual bool CanContainChildren => false;

    /// <summary>
    /// Indicates whether the aspect ratio of the element should be maintained during resizing
    /// </summary>
    public bool LockAspectRatio { get; set; }

    /// <summary>
    /// Collection of child elements
    /// </summary>
    public virtual IList<IPageElement> Childrens =>
        CanContainChildren ? _children : Array.Empty<IPageElement>();

    /// <summary>
    /// Internal children collection
    /// </summary>
    protected readonly List<IPageElement> _children = new();

    /// <summary>
    /// Initializes a new instance of PageElement
    /// </summary>
    /// <param name="name">Element name</param>
    protected PageElement(string name = "Element")
    {
        Id = Guid.CreateVersion7();
        Name = name;
        X = 0;
        Y = 0;
        Width = 100;
        Height = 100;
        Rotation = 0;
        Opacity = 1.0;
        IsVisible = true;
        IsLocked = false;
        ZOrder = 0;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        LockAspectRatio = true;
    }

    /// <summary>
    /// Renders the element using the provided graphics context
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    public void Render(IGraphicsContext graphics)
    {
        if (!IsVisible || graphics == null)
            return;

        graphics.Save();
        try
        {
            // Apply transformations
            graphics.Translate((float)X, (float)Y);

            if (Math.Abs(Rotation) > 0.001)
                graphics.Rotate((float)Rotation);

            // Apply opacity by modifying paint alpha
            RenderCore(graphics);
        }
        finally
        {
            graphics.Restore();
        }
    }

    private SKRect[] handleRects = new SKRect[8]; // 8 handle: 4 angoli, 4 lati
    private readonly float HandleSize = 10f;
    private readonly float HandleHitTestSize = 16f;
    
    public int? HitTestHandle(double canvasX, double canvasY)
    {
        for (int i = 0; i < handleRects.Length; i++)
        {
            var rect = handleRects[i];
            var hitRect = new SKRect(rect.Left - HandleHitTestSize/2, rect.Top - HandleHitTestSize/2, rect.Right + HandleHitTestSize/2, rect.Bottom + HandleHitTestSize/2);
            if (hitRect.Contains((float)canvasX, (float)canvasY))
                return i;
        }
        return null;
    }
    
    /// <summary>
    /// Core rendering implementation - renders self, then children
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    protected virtual void RenderCore(IGraphicsContext graphics)
    {
        // Render self, then children
        RenderSelf(graphics);
        if (CanContainChildren)
        {
            foreach (var child in Childrens.OrderBy(c => c.ZOrder))
                child.Render(graphics);
        }

        if (IsSelected)
        {
            // Disegna il bordo di selezione tratteggiato blu
            // Assumiamo che il contesto sia SkiaSharp (GraphicsContext)
            if (graphics is PageStudio.Core.Graphics.GraphicsContext skiaContext)
            {
                var canvas = skiaContext.Canvas;
                using var selectionPaint = new SkiaSharp.SKPaint
                {
                    Color = SkiaSharp.SKColors.DeepSkyBlue,
                    Style = SkiaSharp.SKPaintStyle.Stroke,
                    StrokeWidth = 2,
                    PathEffect = SkiaSharp.SKPathEffect.CreateDash(new float[] { 3, 3 }, 0)
                };
                canvas.DrawRect(0, 0, (float)Width, (float)Height, selectionPaint);
                
                var x = 0f;
                var y = 0f;
                var w = (float)this.Width;
                var h = (float)this.Height;
                // Calcola le posizioni degli 8 handle
                handleRects[0] = new SKRect(x - HandleSize/2, y - HandleSize/2, x + HandleSize/2, y + HandleSize/2); // top-left
                handleRects[1] = new SKRect(x + w/2 - HandleSize/2, y - HandleSize/2, x + w/2 + HandleSize/2, y + HandleSize/2); // top
                handleRects[2] = new SKRect(x + w - HandleSize/2, y - HandleSize/2, x + w + HandleSize/2, y + HandleSize/2); // top-right
                handleRects[3] = new SKRect(x + w - HandleSize/2, y + h/2 - HandleSize/2, x + w + HandleSize/2, y + h/2 + HandleSize/2); // right
                handleRects[4] = new SKRect(x + w - HandleSize/2, y + h - HandleSize/2, x + w + HandleSize/2, y + h + HandleSize/2); // bottom-right
                handleRects[5] = new SKRect(x + w/2 - HandleSize/2, y + h - HandleSize/2, x + w/2 + HandleSize/2, y + h + HandleSize/2); // bottom
                handleRects[6] = new SKRect(x - HandleSize/2, y + h - HandleSize/2, x + HandleSize/2, y + h + HandleSize/2); // bottom-left
                handleRects[7] = new SKRect(x - HandleSize/2, y + h/2 - HandleSize/2, x + HandleSize/2, y + h/2 + HandleSize/2); // left

                var handlePaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill };
                var handleBorder = new SKPaint { Color = SKColors.DeepSkyBlue, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
                for (int i = 0; i < 8; i++)
                {
                    canvas.DrawRect(handleRects[i], handlePaint);
                    canvas.DrawRect(handleRects[i], handleBorder);
                }

            }
            // Se in futuro ci sono altri backend, aggiungere qui il supporto
        }
    }

    /// <summary>
    /// Abstract method to render the element itself (without children)
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    protected abstract void RenderSelf(IGraphicsContext graphics);

    /// <summary>
    /// Creates a deep copy of this element
    /// </summary>
    /// <returns>Cloned element</returns>
    public abstract IPageElement Clone();

    /// <summary>
    /// Updates the ModifiedAt timestamp
    /// </summary>
    protected void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an element to the layer
    /// </summary>
    /// <param name="element">Element to add</param>
    public virtual void AddChildren(IPageElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!CanContainChildren)
            throw new InvalidOperationException("This element cannot contain children");

        if (!_children.Contains(element))
        {
            _children.Add(element);
            UpdateModifiedTime();
        }
    }

    /// <summary>
    /// Removes an element from the layer
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    public virtual bool RemoveChildren(Guid elementId)
    {
        if (!CanContainChildren)
            return false;

        var element = _children.FirstOrDefault(e => e.Id == elementId);
        if (element != null)
        {
            _children.Remove(element);
            UpdateModifiedTime();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets an element by its ID
    /// </summary>
    /// <param name="elementId">Element ID</param>
    /// <returns>The element if found, null otherwise</returns>
    public virtual IPageElement GetChildren(Guid elementId)
    {
        var ret = _children.FirstOrDefault(e => e.Id == elementId);
        Guard.Against.Null(ret);
        return ret;
    }

    /// <summary>
    /// Gets elements sorted by Z-order (lowest to highest)
    /// </summary>
    /// <returns>Elements sorted by Z-order</returns>
    public virtual IEnumerable<IPageElement> GetElementsByZOrder()
    {
        if (!CanContainChildren)
            return Enumerable.Empty<IPageElement>();

        return _children.OrderBy(e => e.ZOrder);
    }

    /// <summary>
    /// Gets elements at a specific position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Elements at the specified position</returns>
    public virtual IEnumerable<IPageElement> GetElementsAtPosition(double x, double y)
    {
        if (!CanContainChildren)
            return Enumerable.Empty<IPageElement>();

        return _children.Where(e => x >= e.X && x <= e.X + e.Width &&
                                    y >= e.Y && y <= e.Y + e.Height);
    }

    /// <summary>
    /// Clears all elements from the layer
    /// </summary>
    public virtual void ClearElements()
    {
        if (!CanContainChildren)
            return;

        _children.Clear();
        UpdateModifiedTime();
    }

    /// <summary>
    /// Indicates if the element is currently selected (for UI rendering, e.g. border highlight)
    /// </summary>
    private bool _isSelected;
    public virtual bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }
}