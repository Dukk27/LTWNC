using System.Threading.Tasks;
using WNC_G13.Models;
using System.Collections.Generic;

namespace WNC_G13.Repositories
{
    public interface IProgressReportRepository
    {
        // Thêm một báo cáo tiến độ mới
        Task AddProgressReportAsync(ProgressReport report);

        // Lấy một báo cáo tiến độ theo Id
        Task<ProgressReport?> GetProgressReportByIdAsync(int id);

        // Cập nhật báo cáo tiến độ
        Task UpdateProgressReportAsync(ProgressReport report);

        // Lấy tất cả các báo cáo tiến độ cho một dự án
        Task<IEnumerable<ProgressReport>> GetProgressReportsByProjectIdAsync(int projectId);

        // Lấy tất cả báo cáo tiến độ của người dùng
        Task<IEnumerable<ProgressReport>> GetProgressReportsByUserIdAsync(int userId);
        Task<IEnumerable<ProgressReport>> GetAllProgressReportsAsync();  
        Task DeleteProgressReportAsync(int id);
        Task<int> GetUnreadReportsCountForAdminAsync();
    }
}
