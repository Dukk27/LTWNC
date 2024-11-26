using WNC_G13.Models;

public interface IDocumentRepository
{
    Task<List<Document>> GetDocumentsByProjectAsync(int projectId);
    Task<List<Document>> GetDocumentsByTaskAsync(int taskId);
    Task<Document?> GetDocumentByIdAsync(int id);
    Task AddDocumentAsync(Document document);
    Task DeleteDocumentAsync(int id);
    Task<List<Document>> GetAllDocumentsAsync();
}
