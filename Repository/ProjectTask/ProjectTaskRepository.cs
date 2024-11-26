using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class ProjectTaskRepository : IProjectTaskRepository
    {
        private readonly WNCG13Context _context;

        public ProjectTaskRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _context.ProjectTasks.Where(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> SearchTasksByProjectIdAsync(int projectId, string searchTerm)
        {
            // Kiểm tra chuỗi tìm kiếm
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await GetTasksByProjectIdAsync(projectId);
            }

            // Truy vấn với LIKE và hỗ trợ Unicode
            return await _context.ProjectTasks
                .Where(t => t.ProjectId == projectId && EF.Functions.Like(t.TaskName, $"%{searchTerm}%"))
                .ToListAsync();
        }

        public async Task<ProjectTask> GetByIdAsync(int id)
        {
            return await _context.ProjectTasks.FindAsync(id);
        }

        public async Task AddAsync(ProjectTask task)
        {
            await _context.ProjectTasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProjectTask task)
        {
            _context.ProjectTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await GetByIdAsync(id);
            if (task != null)
            {
                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProjectTask>> GetAllTasksAsync()
        {
            return await _context.ProjectTasks.Include(t => t.Project).ToListAsync();
        }

    }
}
