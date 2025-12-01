using PageStudio.Core.Models;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Models;

/// <summary>
/// Model for adding pages to a document, containing all necessary parameters
/// </summary>
public class AddPageModel
{
    /// <summary>
    /// Selected page format for new pages
    /// </summary>
    public PageFormat SelectedPageFormat { get; set; }

    /// <summary>
    /// Selected orientation for new pages
    /// </summary>
    public PageOrientation SelectedOrientation { get; set; }

    /// <summary>
    /// Number of pages to add
    /// </summary>
    public int NumberOfPagesToAdd { get; set; }

    /// <summary>
    /// Available page formats to choose from
    /// </summary>
    public List<PageFormat> AvailablePageFormats { get; set; }

    /// <summary>
    /// Initializes a new instance of AddPageModel with default values
    /// </summary>
    public AddPageModel()
    {
        SelectedPageFormat = PageFormat.Create(StandardPageFormat.A4, PageOrientation.Portrait);
        SelectedOrientation = PageOrientation.Portrait;
        NumberOfPagesToAdd = 1;
        AvailablePageFormats = PageFormat.GetAllStandardFormats();
    }

    /// <summary>
    /// Initializes a new instance of AddPageModel with specified values
    /// </summary>
    public AddPageModel(PageFormat selectedPageFormat, PageOrientation selectedOrientation, int numberOfPages)
    {
        SelectedPageFormat = selectedPageFormat;
        SelectedOrientation = selectedOrientation;
        NumberOfPagesToAdd = numberOfPages;
        AvailablePageFormats = PageFormat.GetAllStandardFormats();
    }

    /// <summary>
    /// Updates the selected page format with the current orientation
    /// </summary>
    public void UpdatePageFormatWithOrientation()
    {
        SelectedPageFormat = PageFormat.Create(SelectedPageFormat.Format, SelectedOrientation);
    }

    /// <summary>
    /// Resets the model to default values
    /// </summary>
    public void Reset()
    {
        SelectedPageFormat = PageFormat.Create(StandardPageFormat.A4, PageOrientation.Portrait);
        SelectedOrientation = PageOrientation.Portrait;
        NumberOfPagesToAdd = 1;
    }
}