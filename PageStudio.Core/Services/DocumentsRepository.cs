using Mediator;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Core.Services;

public class DocumentsRepository(IMediator mediator) : IDocumentsRepository
{
    private readonly Dictionary<Guid, Document> _documents = new();

    public IReadOnlyCollection<Document> Documents => _documents.Values;

    public Document? Get(Guid id) => _documents.GetValueOrDefault(id);

    public Document Create(string name)
    {
        var doc = new Document(mediator, name);
        _documents[doc.Id] = doc;
        return doc;
    }

    public void Close(Guid id)
    {
        _documents.Remove(id);
    }
}