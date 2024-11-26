using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly WNCG13Context _context;

        public ProjectRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetProjectsByOrganizationIdAsync(int organizationId)
        {
            return await _context
                .Projects.Where(p => p.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject != null)
            {
                existingProject.Name = project.Name;
                existingProject.Description = project.Description;
                existingProject.OrganizationId = project.OrganizationId;
                existingProject.Status = project.Status;
                // Giữ lại giá trị CreatedBy từ dữ liệu cũ
                existingProject.CreatedBy = existingProject.CreatedBy;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetUsersInProjectAsync(int projectId)
        {
            return await _context.Users
                .Where(u => u.UserProjects.Any(up => up.ProjectId == projectId))
                .Include(u => u.UserProjects)
                .ThenInclude(up => up.Project)
                .ToListAsync();
        }


        public async Task AddUserToProjectAsync(int projectId, int userId, bool isPm)
        {
            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsPm = isPm,
            };

            await _context.UserProjects.AddAsync(userProject);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserProject>> GetUserProjectsByProjectIdAsync(int projectId)
        {
            return await _context.UserProjects.Where(up => up.ProjectId == projectId).ToListAsync();
        }
        
        
    }
}
