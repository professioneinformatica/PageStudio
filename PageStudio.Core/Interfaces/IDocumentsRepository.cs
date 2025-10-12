namespace PageStudio.Core.Interfaces;

public interface IDocumentsRepository
{
    IReadOnlyCollection<IDocument> Documents { get; }
    IDocument? Get(Guid id);
    IDocument Create(string name);
    void Close(Guid id);
}
