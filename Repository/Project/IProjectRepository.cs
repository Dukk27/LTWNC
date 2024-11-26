using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetProjectsByOrganizationIdAsync(int organizationId);
        Task<Project?> GetByIdAsync(int id);
        Task AddAsync(Project project);
        Task UpdateAsync(Project project); // Cập nhật dự án
        Task DeleteAsync(int id); // Xóa dự án
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task AddUserToProjectAsync(int projectId, int userId, bool isPm);
        Task<List<User>> GetUsersInProjectAsync(int projectId);
        Task<List<UserProject>> GetUserProjectsByProjectIdAsync(int projectId);
        
    }
}
