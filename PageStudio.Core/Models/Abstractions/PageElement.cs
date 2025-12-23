using Ardalis.GuardClauses;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Models.Page;
using PageStudio.Core.Features.ParametricProperties;
using SkiaSharp;

namespace PageStudio.Core.Models.Abstractions;

/// <summary>
/// Abstract base implementation of IPageElement interface for all page elements
/// </summary>
public abstract class PageElement : IPageElement
{
    private readonly IEventPublisher _eventPublisher;
    
    #region Properties
    
    /// <summary>
    /// Parametric property representing the horizontal offset of the element.
    /// </summary>
    public DynamicProperty<double> X { get; set; }

    /// <summary>
    /// Parametric property representing the vertical offset of the element.
    /// </summary>
    public DynamicProperty<double> Y { get; set; }

    /// <summary>
    /// Parametric property representing the width of the element.
    /// </summary>
    public DynamicProperty<double> Width { get; set; }

    /// <summary>
    /// Parametric property representing the height of the element.
    /// </summary>
    public DynamicProperty<double> Height { get; set; }

    /// <summary>
    /// Parametric property representing the rotation angle of the element.
    /// </summary>
    public DynamicProperty<double> Rotation { get; set; }

    /// <summary>
    /// Parametric property representing the opacity of the element.
    /// </summary>
    public DynamicProperty<double> Opacity { get; set; }

    /// <summary>
    /// Parametric property indicating whether the element is visible.
    /// </summary>
    public DynamicProperty<bool> IsVisible { get; set; }

    /// <summary>
    /// Parametric property indicating whether the element is locked.
    /// </summary>
    public DynamicProperty<bool> IsLocked { get; set; }

    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Element name/title
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (field == value) return;
            
            if (!Page.Document.ParametricEngine.Symbols.IsSymbolNameAvailable(value, Id))
            {
                throw new InvalidOperationException($"The name '{value}' is already in use by another element.");
            }
            Page.Document.ParametricEngine.RegisterElement(value, Id);
            field = value;
        }
    }

    /// <summary>
    /// Indicates whether the element should be excluded from the document structure.
    /// When set to true, the element will not be part of the logical structure of the document
    /// </summary>
    public bool HideFromDocumentStructure { get; set; }

    /// <summary>
    /// Z-order of the element (higher values are on top)
    /// </summary>
    public int ZIndex
    {
        get => Parent?.GetChildIndex(this) ?? 0;
        set
        {
            var oldZIndex = this.ZIndex;
            Parent?.MoveChildToIndex(this, value);
            _eventPublisher.Publish(new ZIndexChangedMessage(this, oldZIndex));
        }
    }
    
    
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

    public IPageElement Parent { get; set; }

    /// <summary>
    /// Indicates whether the aspect ratio of the element should be maintained during resizing
    /// </summary>
    public bool LockAspectRatio { get; set; }

    /// <summary>
    /// Aspect ratio of the element
    /// </summary>
    public double AspectRatio => (Width.Value != 0) ? (Width.Value / (Height.Value != 0 ? Height.Value : 1)) : 1;

    /// <summary>
    /// Collection of child elements
    /// </summary>
    public virtual IReadOnlyList<IPageElement> Children =>
        CanContainChildren ? _children.AsReadOnly() : Array.Empty<IPageElement>().AsReadOnly();

    /// <summary>
    /// Internal children collection
    /// </summary>
    protected readonly List<IPageElement> _children = new();

    public IPage Page { get; }

    #endregion

    public int GetChildIndex(IPageElement child) => _children.IndexOf(child);
    
    protected void SetDimension(double width, double height)
    {
        Width.Value = width;
        Height.Value = height;
    }

    /// <summary>
    /// Initializes a new instance of PageElement
    /// </summary>
    /// <param name="eventPublisher"></param>
    /// <param name="page"></param>
    /// <param name="name">Element name</param>
    protected PageElement(IEventPublisher eventPublisher, IPage page, string name = "Element")
    {
        _eventPublisher = eventPublisher;
        Page = page;
        Id = Guid.CreateVersion7();
        Name = name;

        var engine = Page.Document.ParametricEngine;
        
        X = engine.CreateProperty(Id, "X", 0.0);
        Y = engine.CreateProperty(Id, "Y", 0.0);
        Width = engine.CreateProperty(Id, "Width", 100.0);
        Height = engine.CreateProperty(Id, "Height", 100.0);
        Rotation = engine.CreateProperty(Id, "Rotation", 0.0);
        Opacity = engine.CreateProperty(Id, "Opacity", 1.0);
        IsVisible = engine.CreateProperty(Id, "IsVisible", true);
        IsLocked = engine.CreateProperty(Id, "IsLocked", false);

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
        Guard.Against.Null(graphics);
        if (!IsVisible.Value)
            return;

        graphics.Save();
        try
        {
            // Apply transformations
            graphics.Translate((float)X.Value, (float)Y.Value);

            if (Math.Abs(Rotation.Value) > 0.001)
                graphics.Rotate((float)Rotation.Value);

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
            var hitRect = new SKRect(rect.Left - HandleHitTestSize / 2, rect.Top - HandleHitTestSize / 2, rect.Right + HandleHitTestSize / 2, rect.Bottom + HandleHitTestSize / 2);
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
            foreach (var child in Children.OrderBy(c => c.ZIndex))
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

                var w = (float)Width.Value;
                var h = (float)Height.Value;
                canvas.DrawRect(0, 0, w, h, selectionPaint);

                var x = 0f;
                var y = 0f;
                // Calcola le posizioni degli 8 handle
                handleRects[0] = new SKRect(x - HandleSize / 2, y - HandleSize / 2, x + HandleSize / 2, y + HandleSize / 2); // top-left
                handleRects[1] = new SKRect(x + w / 2 - HandleSize / 2, y - HandleSize / 2, x + w / 2 + HandleSize / 2, y + HandleSize / 2); // top
                handleRects[2] = new SKRect(x + w - HandleSize / 2, y - HandleSize / 2, x + w + HandleSize / 2, y + HandleSize / 2); // top-right
                handleRects[3] = new SKRect(x + w - HandleSize / 2, y + h / 2 - HandleSize / 2, x + w + HandleSize / 2, y + h / 2 + HandleSize / 2); // right
                handleRects[4] = new SKRect(x + w - HandleSize / 2, y + h - HandleSize / 2, x + w + HandleSize / 2, y + h + HandleSize / 2); // bottom-right
                handleRects[5] = new SKRect(x + w / 2 - HandleSize / 2, y + h - HandleSize / 2, x + w / 2 + HandleSize / 2, y + h + HandleSize / 2); // bottom
                handleRects[6] = new SKRect(x - HandleSize / 2, y + h - HandleSize / 2, x + HandleSize / 2, y + h + HandleSize / 2); // bottom-left
                handleRects[7] = new SKRect(x - HandleSize / 2, y + h / 2 - HandleSize / 2, x + HandleSize / 2, y + h / 2 + HandleSize / 2); // left

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
    public virtual void AddChild(IPageElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!CanContainChildren)
            throw new InvalidOperationException("This element cannot contain children");

        // rimuovo l'elemento dal parent precedente nel caso in cui fosse già presente
        element.Parent?.RemoveChild(element);

        element.Parent = this;
        _children.Add(element);
        UpdateModifiedTime();
    }

    public bool RemoveChild(IPageElement element)
    {
        Guard.Against.Null(element);

        if (!CanContainChildren)
            return false;

        if (_children.Remove(element))
        {
            element.Parent = null;
            UpdateModifiedTime();
            return true;
        }

        return false;
    }


    /// <summary>
    /// Removes an element from the layer
    /// </summary>
    /// <param name="elementId">ID of the element to remove</param>
    /// <returns>True if element was removed, false otherwise</returns>
    public virtual bool RemoveChild(Guid elementId)
    {
        if (!CanContainChildren)
            return false;

        var element = _children.FirstOrDefault(e => e.Id == elementId);
        if (element != null)
        {
            return this.RemoveChild(element);
        }

        return false;
    }

    /// <summary>
    /// Moves a child element to a specified index within the children collection.
    /// </summary>
    /// <param name="element">The child element to move.</param>
    /// <param name="newIndex">The target index to move the element to.</param>
    public void MoveChildToIndex(PageElement element, int newIndex)
    {
        if (!CanContainChildren)
        {
            return;
        }

        var oldIndex = _children.IndexOf(element);
        if (oldIndex < 0)
            return;

        newIndex = Clamp(newIndex, 0, _children.Count - 1);

        if (oldIndex == newIndex)
            return;

        _children.RemoveAt(oldIndex);

        // Se rimuovo un elemento prima della nuova posizione,
        // devo decrementare il target per mantenere la coerenza.
        if (newIndex > oldIndex)
            newIndex--;

        _children.Insert(newIndex, element);
    }

    private static int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
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

        return _children.OrderBy(e => e.ZIndex);
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

        return _children.Where(e => x >= e.X.Value && x <= e.X.Value + e.Width.Value &&
                                    y >= e.Y.Value && y <= e.Y.Value + e.Height.Value);
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
    public virtual bool IsSelected { get; set; }
}