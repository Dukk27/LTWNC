using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class ProgressReportRepository : IProgressReportRepository
    {
        private readonly WNCG13Context _context;

        public ProgressReportRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProgressReport>> GetAllProgressReportsAsync()
        {
            return await _context.ProgressReports
                .Include(pr => pr.User)   // Bao gồm thông tin người gửi
                .Include(pr => pr.Project)  // Bao gồm thông tin dự án
                .ToListAsync();  // Lấy tất cả báo cáo tiến độ
        }

        // Thêm một báo cáo tiến độ mới
        public async Task AddProgressReportAsync(ProgressReport report)
        {
            await _context.ProgressReports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        // Lấy một báo cáo tiến độ theo Id
        public async Task<ProgressReport?> GetProgressReportByIdAsync(int id)
        {
            return await _context.ProgressReports
                .Include(pr => pr.User) // Người gửi báo cáo
                .Include(pr => pr.Approver) // Người duyệt báo cáo
                .Include(pr => pr.Project) // Dự án liên quan
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }

        // Cập nhật báo cáo tiến độ
        public async Task UpdateProgressReportAsync(ProgressReport report)
        {
            _context.ProgressReports.Update(report);
            await _context.SaveChangesAsync();
        }

        // Lấy tất cả các báo cáo tiến độ cho một dự án
        public async Task<IEnumerable<ProgressReport>> GetProgressReportsByProjectIdAsync(int projectId)
        {
            return await _context.ProgressReports
                .Include(pr => pr.User)
                .Include(pr => pr.Project)
                .Where(pr => pr.ProjectId == projectId)
                .ToListAsync();
        }

        // Lấy tất cả báo cáo tiến độ của người dùng
        public async Task<IEnumerable<ProgressReport>> GetProgressReportsByUserIdAsync(int userId)
        {
            return await _context.ProgressReports
                .Include(pr => pr.Project)
                .Where(pr => pr.UserId == userId)
                .ToListAsync();
        }

        public async Task DeleteProgressReportAsync(int id)
        {
            var report = await _context.ProgressReports.FindAsync(id);
            if (report != null)
            {
                _context.ProgressReports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadReportsCountForAdminAsync()
        {
            return await _context.ProgressReports
                                 .Where(r => r.Status == "Pending") // Chỉ lấy báo cáo có trạng thái "Pending"
                                 .CountAsync();
        }

    }
}
