using PageStudio.Core.Models.Documents;

namespace PageStudio.Core.Interfaces;

public interface IDocumentsRepository
{
    IReadOnlyCollection<IDocument> Documents { get; }
    IDocument? Get(Guid id);
    IDocument Create(string name);
    void Close(Guid id);
    public IDocument? CurrentDocument { get; set; }
    
}
