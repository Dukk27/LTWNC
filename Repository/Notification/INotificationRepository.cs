using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface INotificationRepository
    {
        // Thêm thông báo mới vào cơ sở dữ liệu
        Task AddNotificationAsync(Notification notification);

        // Lấy tất cả thông báo của người dùng
        Task<List<Notification>> GetUserNotificationsAsync(int userId);

        // Lấy thông báo theo Id
        Task<Notification> GetNotificationByIdAsync(int id);

        // Cập nhật thông báo
        Task UpdateNotificationAsync(Notification notification);

        // Xóa thông báo
        Task DeleteNotificationAsync(int id);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task<int> GetUnreadNotificationCountAsync(int userId);
    }
}

