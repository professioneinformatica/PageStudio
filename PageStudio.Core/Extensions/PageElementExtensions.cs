using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.ContainerPageElements;

namespace PageStudio.Core.Extensions;

public static class PageElementExtensions
{
    public static string GetFQName(this IPageElement element)
    {
        var parts = new List<string>();
        var current = element;
        
        while (current != null)
        {
            parts.Insert(0, current.Name);
            current = current.Parent;
        }
        
        // Se l'elemento è un Layer o figlio di un Layer, aggiungiamo il nome della pagina se disponibile
        if (element.Page != null)
        {
            parts.Insert(0, element.Page.Name);
        }
        
        return string.Join(".", parts);
    }
}
