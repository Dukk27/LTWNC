using Microsoft.AspNetCore.Mvc;
using WNC_G13.Repositories;
using System.Security.Claims;
using System.Linq;

namespace WNC_G13.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // Lấy tất cả thông báo của người dùng
        public async Task<IActionResult> Index()
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Chuyển userId sang kiểu int
            var userIdInt = int.Parse(userId);

            // Lấy số lượng thông báo chưa đọc
            var unreadCount = await _notificationRepository.GetUnreadNotificationCountAsync(userIdInt);
            ViewBag.UnreadNotificationCount = unreadCount;

            // Lấy danh sách thông báo của người dùng
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userIdInt);

            return View(notifications);
        }

        // Đánh dấu thông báo là đã đọc
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateNotificationAsync(notification);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // Xóa thông báo
        public async Task<IActionResult> Delete(int notificationId)
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            if (notification != null)
            {
                await _notificationRepository.DeleteNotificationAsync(notificationId);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // Đánh dấu tất cả thông báo là đã đọc
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            var userIdInt = int.Parse(userId);
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userIdInt);
            foreach (var notification in notifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateNotificationAsync(notification);
            }

            return Json(new { success = true });
        }

        // Xóa tất cả thông báo
        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            var userIdInt = int.Parse(userId);
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userIdInt);
            foreach (var notification in notifications)
            {
                await _notificationRepository.DeleteNotificationAsync(notification.Id);
            }

            return Json(new { success = true });
        }
    }
}
