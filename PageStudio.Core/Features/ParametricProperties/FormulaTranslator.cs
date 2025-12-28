using System.Text.RegularExpressions;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Models.Page;
using PageStudio.Core.Extensions;

namespace PageStudio.Core.Features.ParametricProperties;

public class FormulaTranslator(IDocument document)
{
    // Regex per trovare pattern come Page1.Layer1.TextBox1.Property
    // O semplicemente TextBox1.Property se univoco o nel contesto
    // Cerchiamo identificatori separati da punti.
    private static readonly Regex PathRegex = new(@"\b([a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*)\b", RegexOptions.Compiled);
    
    // Regex per trovare ID normalizzati: [[guid:property]]
    private static readonly Regex IdRegex = new(@"\[\[([0-9a-fA-F-]{36}):([a-zA-Z_][a-zA-Z0-9_]*)\]\]", RegexOptions.Compiled);

    public string Normalize(string displayFormula)
    {
        if (string.IsNullOrWhiteSpace(displayFormula)) return displayFormula;

        return PathRegex.Replace(displayFormula, match =>
        {
            var path = match.Value;
            
            var lastDot = path.LastIndexOf('.');
            if (lastDot == -1) return path;

            var elementPath = path.Substring(0, lastDot);
            var propertyName = path.Substring(lastDot + 1);

            var element = ResolveElementByPath(elementPath);
            if (element != null)
            {
                return $"[[{element.Id}:{propertyName}]]";
            }

            return path;
        });
    }

    public string Denormalize(string internalFormula)
    {
        if (string.IsNullOrWhiteSpace(internalFormula)) return internalFormula;

        return IdRegex.Replace(internalFormula, match =>
        {
            var id = Guid.Parse(match.Groups[1].Value);
            var propertyName = match.Groups[2].Value;

            var element = FindElementById(id);
            if (element != null)
            {
                return $"{element.GetFQName()}.{propertyName}";
            }

            return match.Value;
        });
    }

    private IPageElement? ResolveElementByPath(string path)
    {
        var parts = path.Split('.');
        if (parts.Length == 0) return null;

        // Prova a risolvere partendo dalla pagina
        foreach (var page in document.Pages)
        {
            if (page.Name == parts[0])
            {
                if (parts.Length == 1) return null; // Pagina non è IPageElement

                // Cerca nei layer della pagina
                var layer = page.Layers.FirstOrDefault(l => l.Name == parts[1]);
                if (layer != null)
                {
                    if (parts.Length == 2) return layer;
                    return ResolveRecursive(layer, parts.Skip(2).ToArray());
                }
            }

            // Prova a cercare direttamente nei layer di questa pagina se il primo elemento del path è un layer
            foreach (var layer in page.Layers)
            {
                if (layer.Name == parts[0])
                {
                    if (parts.Length == 1) return layer;
                    var foundRecursive = ResolveRecursive(layer, parts.Skip(1).ToArray());
                    if (foundRecursive != null) return foundRecursive;
                }
                
                // Cerca ricorsivamente in tutto l'albero di questa pagina
                var foundAnywhere = ResolveRecursive(layer, parts);
                if (foundAnywhere != null) return foundAnywhere;
            }
        }

        return null;
    }

    private IPageElement? ResolveRecursive(IPageElement parent, string[] remainingParts)
    {
        if (remainingParts.Length == 0) return parent;

        var child = parent.Children.FirstOrDefault(c => c.Name == remainingParts[0]);
        if (child != null)
        {
            return ResolveRecursive(child, remainingParts.Skip(1).ToArray());
        }

        return null;
    }

    private IPageElement? FindElementById(Guid id)
    {
        foreach (var page in document.Pages)
        {
            var element = page.GetElement(id);
            if (element != null) return element;
            
            // Controlla anche i layer stessi se l'ID è di un layer
            var layer = page.Layers.FirstOrDefault(l => l.Id == id);
            if (layer != null) return layer;
        }
        return null;
    }
}
