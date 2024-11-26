using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly WNCG13Context _context;

        public NotificationRepository(WNCG13Context context)
        {
            _context = context;
        }

        // Thêm thông báo mới
        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Lấy tất cả thông báo của người dùng
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                                 .Where(n => n.UserId == userId)
                                 .OrderByDescending(n => n.CreatedAt)
                                 .ToListAsync();
        }

        // Lấy thông báo theo Id
        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications
                                 .FirstOrDefaultAsync(n => n.Id == id);
        }

        // Cập nhật thông báo (đánh dấu là đã đọc)
        public async Task UpdateNotificationAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        // Xóa thông báo
        public async Task DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public Task<dynamic> GetUserNotificationsAsync(int? userId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead) // Lọc thông báo chưa đọc
                .CountAsync(); // Đếm số lượng thông báo chưa đọc
        }

    }
}

