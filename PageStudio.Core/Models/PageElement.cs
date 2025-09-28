using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models;

/// <summary>
/// Base implementation of IPageElement interface for all page elements
/// </summary>
public class PageElement : IPageElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    public string Id { get; }
    
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
    /// Initializes a new instance of PageElement
    /// </summary>
    /// <param name="name">Element name</param>
    public PageElement(string name = "Element")
    {
        Id = Guid.NewGuid().ToString();
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
    }

    /// <summary>
    /// Renders the element using the provided graphics context
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    public virtual void Render(IGraphicsContext graphics)
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
            // This is a basic implementation - derived classes should override for specific rendering
            RenderCore(graphics);
        }
        finally
        {
            graphics.Restore();
        }
    }

    /// <summary>
    /// Core rendering method to be overridden by derived classes
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    protected virtual void RenderCore(IGraphicsContext graphics)
    {
        // Base implementation does nothing - override in derived classes
    }

    /// <summary>
    /// Creates a deep copy of this element
    /// </summary>
    /// <returns>Cloned element</returns>
    public virtual IPageElement Clone()
    {
        var clone = new PageElement(Name)
        {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Rotation = Rotation,
            Opacity = Opacity,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            ZOrder = ZOrder,
            ModifiedAt = ModifiedAt
        };

        return clone;
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp
    /// </summary>
    protected void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}