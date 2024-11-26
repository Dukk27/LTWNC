using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WNCG13Context _context;

        public UserRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return false; // Email đã tồn tại

            user.CreatedAt = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            return user; // Trả về null nếu không tìm thấy người dùng
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            //return await _context.Users.FindAsync(id);
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Password = newPassword; // Thay đổi mật khẩu người dùng
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<User>> GetUsersNotInProjectAsync(int projectId)
        {
            // Lấy danh sách người dùng chưa thuộc dự án
            return await _context.Users
                .Where(u => !u.UserProjects.Any(up => up.ProjectId == projectId))
                .ToListAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<int>> GetOrganizationsByUserIdAsync(int userId)
        {
            // Truy vấn bảng UserOrganization để lấy tất cả tổ chức của người dùng
            var organizations = await _context.UserOrganizations
                .Where(uo => uo.UserId == userId)
                .Select(uo => uo.OrganizationId)
                .ToListAsync();

            return organizations;
        }

        public async Task<List<User>> GetUsersByOrganizationIdAsync(int organizationId)
        {
            // Lấy danh sách UserId từ bảng UserOrganization theo OrganizationId
            var userIdsInOrganization = await _context.UserOrganizations
                .Where(uo => uo.OrganizationId == organizationId)
                .Select(uo => uo.UserId)
                .ToListAsync();

            // Trả về danh sách người dùng từ bảng Users, có UserId nằm trong danh sách vừa lấy
            return await _context.Users
                .Where(u => userIdsInOrganization.Contains(u.Id))
                .ToListAsync();
        }


    }
}
