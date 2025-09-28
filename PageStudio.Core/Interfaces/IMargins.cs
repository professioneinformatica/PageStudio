namespace PageStudio.Core.Interfaces;

/// <summary>
/// Represents margins for a page or element
/// </summary>
public interface IMargins
{
    /// <summary>
    /// Top margin in points
    /// </summary>
    double Top { get; set; }
    
    /// <summary>
    /// Right margin in points
    /// </summary>
    double Right { get; set; }
    
    /// <summary>
    /// Bottom margin in points
    /// </summary>
    double Bottom { get; set; }
    
    /// <summary>
    /// Left margin in points
    /// </summary>
    double Left { get; set; }
    
    /// <summary>
    /// Sets all margins to the same value
    /// </summary>
    /// <param name="value">Margin value in points</param>
    void SetAll(double value);
    
    /// <summary>
    /// Sets horizontal margins (left and right)
    /// </summary>
    /// <param name="value">Margin value in points</param>
    void SetHorizontal(double value);
    
    /// <summary>
    /// Sets vertical margins (top and bottom)
    /// </summary>
    /// <param name="value">Margin value in points</param>
    void SetVertical(double value);
}