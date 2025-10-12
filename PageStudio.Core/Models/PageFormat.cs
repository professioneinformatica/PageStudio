using System.Collections.Generic;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Core.Models;

/// <summary>
/// Enumeration of standard page formats
/// </summary>
public enum StandardPageFormat
{
    A4,
    A3,
    A5,
    Letter,
    Legal,
    Tabloid,
    Custom
}

/// <summary>
/// Page orientation enumeration
/// </summary>
public enum PageOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Page format information containing dimensions and metadata
/// </summary>
public class PageFormat
{
    public StandardPageFormat Format { get; set; }
    public PageOrientation Orientation { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public PageFormat(StandardPageFormat format, PageOrientation orientation, double width, double height, string name, string description)
    {
        Format = format;
        Orientation = orientation;
        Width = width;
        Height = height;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Gets the actual width considering orientation
    /// </summary>
    public double ActualWidth => Orientation == PageOrientation.Portrait ? Width : Height;

    /// <summary>
    /// Gets the actual height considering orientation
    /// </summary>
    public double ActualHeight => Orientation == PageOrientation.Portrait ? Height : Width;

    /// <summary>
    /// Gets predefined page formats with their dimensions in points (1 point = 1/72 inch)
    /// </summary>
    public static Dictionary<StandardPageFormat, (double width, double height, string name, string description)> StandardFormats = new()
    {
        { StandardPageFormat.A4, (595, 842, "A4", "210 × 297 mm") },
        { StandardPageFormat.A3, (842, 1191, "A3", "297 × 420 mm") },
        { StandardPageFormat.A5, (420, 595, "A5", "148 × 210 mm") },
        { StandardPageFormat.Letter, (612, 792, "Letter", "8.5 × 11 inch") },
        { StandardPageFormat.Legal, (612, 1008, "Legal", "8.5 × 14 inch") },
        { StandardPageFormat.Tabloid, (792, 1224, "Tabloid", "11 × 17 inch") }
    };

    /// <summary>
    /// Creates a PageFormat instance for a standard format and orientation
    /// </summary>
    public static PageFormat Create(StandardPageFormat format, PageOrientation orientation)
    {
        if (format == StandardPageFormat.Custom)
        {
            throw new ArgumentException("Use CreateCustom method for custom formats", nameof(format));
        }

        var standardFormat = StandardFormats[format];
        return new PageFormat(
            format,
            orientation,
            standardFormat.width,
            standardFormat.height,
            $"{standardFormat.name} ({orientation})",
            $"{standardFormat.description} - {orientation}"
        );
    }

    /// <summary>
    /// Creates a custom PageFormat instance
    /// </summary>
    public static PageFormat CreateCustom(double width, double height, string name, string description)
    {
        return new PageFormat(
            StandardPageFormat.Custom,
            PageOrientation.Portrait, // Default orientation for custom
            width,
            height,
            name,
            description
        );
    }

    /// <summary>
    /// Gets all available page formats with portrait orientation only
    /// </summary>
    public static List<PageFormat> GetAllStandardFormats()
    {
        var formats = new List<PageFormat>();
        
        foreach (var format in StandardFormats.Keys)
        {
            formats.Add(Create(format, PageOrientation.Portrait));
        }

        return formats;
    }

    /// <summary>
    /// Converts points to inches
    /// </summary>
    public static double PointsToInches(double points)
    {
        return points / 72.0;
    }

    /// <summary>
    /// Converts inches to points
    /// </summary>
    public static double InchesToPoints(double inches)
    {
        return inches * 72.0;
    }

    /// <summary>
    /// Converts points to centimeters
    /// </summary>
    public static double PointsToCentimeters(double points)
    {
        return PointsToInches(points) * 2.54;
    }

    /// <summary>
    /// Converts centimeters to points
    /// </summary>
    public static double CentimetersToPoints(double centimeters)
    {
        return InchesToPoints(centimeters / 2.54);
    }

    /// <summary>
    /// Gets the width in the specified unit of measure considering DPI
    /// </summary>
    public double GetWidthInUnit(UnitOfMeasure unit, int dpi = 72)
    {
        var actualWidth = ActualWidth;
        
        return unit switch
        {
            UnitOfMeasure.Inches => PointsToInches(actualWidth),
            UnitOfMeasure.Centimeters => PointsToCentimeters(actualWidth),
            _ => actualWidth // Return points as fallback
        };
    }

    /// <summary>
    /// Gets the height in the specified unit of measure considering DPI
    /// </summary>
    public double GetHeightInUnit(UnitOfMeasure unit, int dpi = 72)
    {
        var actualHeight = ActualHeight;
        
        return unit switch
        {
            UnitOfMeasure.Inches => PointsToInches(actualHeight),
            UnitOfMeasure.Centimeters => PointsToCentimeters(actualHeight),
            _ => actualHeight // Return points as fallback
        };
    }

    /// <summary>
    /// Gets the actual pixel width based on DPI
    /// </summary>
    public double GetPixelWidth(int dpi = 72)
    {
        return PointsToInches(ActualWidth) * dpi;
    }

    /// <summary>
    /// Gets the actual pixel height based on DPI
    /// </summary>
    public double GetPixelHeight(int dpi = 72)
    {
        return PointsToInches(ActualHeight) * dpi;
    }

    public override string ToString()
    {
        return Name;
    }
}