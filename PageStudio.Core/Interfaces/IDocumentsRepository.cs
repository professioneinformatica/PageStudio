namespace PageStudio.Core.Interfaces;

public interface IDocumentsRepository
{
    IReadOnlyCollection<Models.Documents.Document> Documents { get; }
    Models.Documents.Document? Get(Guid id);
    Models.Documents.Document Create(string name);
    void Close(Guid id);
}
