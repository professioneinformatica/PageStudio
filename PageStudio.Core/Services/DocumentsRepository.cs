using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Core.Services;

public class DocumentsRepository(IEventPublisher eventPublisher) : IDocumentsRepository
{
    private readonly Dictionary<Guid, Document> _documents = new();

    public IReadOnlyCollection<IDocument> Documents => _documents.Values;

    public IDocument? Get(Guid id) => _documents.GetValueOrDefault(id);

    public IDocument? CurrentDocument { get; set; }

    public IDocument Create(string name)
    {
        var doc = new Document(eventPublisher, name);
        _documents[doc.Id] = doc;
        this.CurrentDocument = doc;
        return doc;
    }

    public void Close(Guid id)
    {
        _documents.Remove(id);
    }
}