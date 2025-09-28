using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models;

/// <summary>
/// Implementation of IMargins interface for page and element margins
/// </summary>
public class Margins : IMargins
{
    /// <summary>
    /// Top margin in points
    /// </summary>
    public double Top { get; set; }
    
    /// <summary>
    /// Right margin in points
    /// </summary>
    public double Right { get; set; }
    
    /// <summary>
    /// Bottom margin in points
    /// </summary>
    public double Bottom { get; set; }
    
    /// <summary>
    /// Left margin in points
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// Initializes a new instance with zero margins
    /// </summary>
    public Margins()
    {
        Top = Right = Bottom = Left = 0.0;
    }

    /// <summary>
    /// Initializes a new instance with the same margin for all sides
    /// </summary>
    /// <param name="all">Margin value for all sides</param>
    public Margins(double all)
    {
        Top = Right = Bottom = Left = all;
    }

    /// <summary>
    /// Initializes a new instance with specific margins
    /// </summary>
    /// <param name="top">Top margin</param>
    /// <param name="right">Right margin</param>
    /// <param name="bottom">Bottom margin</param>
    /// <param name="left">Left margin</param>
    public Margins(double top, double right, double bottom, double left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    /// <summary>
    /// Sets all margins to the same value
    /// </summary>
    /// <param name="value">Margin value in points</param>
    public void SetAll(double value)
    {
        Top = Right = Bottom = Left = value;
    }

    /// <summary>
    /// Sets horizontal margins (left and right)
    /// </summary>
    /// <param name="value">Margin value in points</param>
    public void SetHorizontal(double value)
    {
        Left = Right = value;
    }

    /// <summary>
    /// Sets vertical margins (top and bottom)
    /// </summary>
    /// <param name="value">Margin value in points</param>
    public void SetVertical(double value)
    {
        Top = Bottom = value;
    }
}