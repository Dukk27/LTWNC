using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly WNCG13Context _context;

        public OrganizationRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .Include(o => o.CreatedByNavigation) // Bao gồm thông tin người tạo (User)
                .Include(o => o.Projects) // Bao gồm thông tin các dự án liên quan
                .ToListAsync();
        }


        public async Task<Organization> GetByIdAsync(int id)
        {
            //return await _context.Organizations.FindAsync(id);
            return await _context.Organizations
                .Include(o => o.CreatedByNavigation) // Bao gồm thông tin người tạo (User)
                .Include(o => o.Projects)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organization> AddAsync(Organization organization)
        {
            try
            {
                organization.Id = 0;
                // Kiểm tra giá trị của đối tượng organization
                System.Diagnostics.Debug.WriteLine($"Organization Name: {organization.Name}");
                System.Diagnostics.Debug.WriteLine($"CreatedBy: {organization.CreatedBy}");

                // Thêm tổ chức vào DbContext
                await _context.Organizations.AddAsync(organization);

                // Ghi log trạng thái của tổ chức sau khi thêm vào
                System.Diagnostics.Debug.WriteLine($"State of Organization: {_context.Entry(organization).State}");

                // Lưu thay đổi vào cơ sở dữ liệu
                int result = await _context.SaveChangesAsync();

                // Kiểm tra kết quả trả về từ SaveChangesAsync
                if (result == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No changes were saved to the database.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{result} changes were saved to the database.");
                }

                // Trả về tổ chức đã thêm
                return organization;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có
                System.Diagnostics.Debug.WriteLine($"Error adding organization: {ex.Message}");
                throw; // Ném lại ngoại lệ nếu cần
            }
        }

        public async Task<Organization> UpdateAsync(Organization organization)
        {
            _context.Entry(organization).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Tìm tổ chức bằng ID
            var organization = await _context.Organizations.FindAsync(id);
            // Nếu không tìm thấy tổ chức, trả về false
            if (organization == null) return false;

            // Xóa tổ chức khỏi DbContext
            _context.Organizations.Remove(organization);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về true nếu xóa thành công
            return true;
        }


    }
}
