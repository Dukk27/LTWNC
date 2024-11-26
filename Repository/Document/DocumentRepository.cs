using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly WNCG13Context _context;

        public DocumentRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetDocumentsByProjectAsync(int projectId)
        {
            return await _context.Documents
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.UploadedByNavigation)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsByTaskAsync(int taskId)
        {
            return await _context.Documents
                .Where(d => d.TaskId == taskId)
                .Include(d => d.UploadedByNavigation)
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.UploadedByNavigation)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Project)
                .Include(d => d.Task)
                .Include(d => d.UploadedByNavigation)
                .ToListAsync(); // Lấy tất cả tài liệu kèm thông tin liên quan
        }
    }
}
