using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IUserRepository
    {
        Task<bool> RegisterUserAsync(User user); // Thêm người dùng mới
        Task<User?> GetUserByEmailAsync(string email); // Lấy người dùng qua email
        Task<User?> LoginUserAsync(string email, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user); // Cập nhật thông tin người dùng
        Task DeleteUserAsync(int id); // Xóa người dùng theo ID
        Task<User> GetUserByIdAsync(int id);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<List<User>> GetUsersNotInProjectAsync(int projectId);
        Task<List<User>> GetAllAsync();
        Task<List<int>> GetOrganizationsByUserIdAsync(int userId);
        Task<List<User>> GetUsersByOrganizationIdAsync(int organizationId);
    }
}
