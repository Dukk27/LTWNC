using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    [Authorize]
    public class ProgressReportController : Controller
    {
        private readonly IProgressReportRepository _progressReportRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public ProgressReportController(
            IProgressReportRepository progressReportRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository)
        {
            _progressReportRepository = progressReportRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }

        // Người dùng gửi báo cáo tiến độ (GET)
        [HttpGet]
        public IActionResult Create(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        // Người dùng gửi báo cáo tiến độ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgressReport model)
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            model.UserId = int.Parse(userId);
            model.CreatedAt = DateTime.Now;

            await _progressReportRepository.AddProgressReportAsync(model);

            // Tạo thông báo cho admin
            var project = await _projectRepository.GetByIdAsync(model.ProjectId);
            var notification = new Notification
            {
                Message = $"Có một báo cáo tiến độ mới từ {User.Identity.Name} cho dự án {project.Name}.",
                UserId = 1, 
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            await _notificationRepository.AddNotificationAsync(notification);

            TempData["SuccessMessage"] = "Báo cáo tiến độ đã được gửi thành công!";
            return RedirectToAction("Index", "ProjectTask", new { projectId = model.ProjectId });
        }

        // Hiển thị danh sách báo cáo tiến độ để admin duyệt
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> ApproveReports()
        {
            var reports = await _progressReportRepository.GetAllProgressReportsAsync();
            return View(reports);
        }

        // Admin duyệt báo cáo tiến độ
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Approve(int reportId)
        {
            var report = await _progressReportRepository.GetProgressReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = "Approved";
            report.ApprovedAt = DateTime.Now;
            report.ApprovedBy = int.Parse(User.FindFirstValue("UserId"));

            await _progressReportRepository.UpdateProgressReportAsync(report);

            // Tạo thông báo cho người dùng
            var notification = new Notification
            {
                Message = $"Báo cáo tiến độ của bạn cho dự án {report.Project.Name} đã được duyệt.",
                UserId = report.UserId,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            await _notificationRepository.AddNotificationAsync(notification);

            TempData["SuccessMessage"] = "Báo cáo tiến độ đã được duyệt!";
            return RedirectToAction("ApproveReports");
        }

        // Admin từ chối báo cáo tiến độ
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Reject(int reportId)
        {
            var report = await _progressReportRepository.GetProgressReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = "Rejected";
            await _progressReportRepository.UpdateProgressReportAsync(report);

            // Tạo thông báo cho người dùng
            var notification = new Notification
            {
                Message = $"Báo cáo tiến độ của bạn cho dự án {report.Project.Name} đã bị từ chối.",
                UserId = report.UserId,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            await _notificationRepository.AddNotificationAsync(notification);

            TempData["ErrorMessage"] = "Báo cáo tiến độ đã bị từ chối!";
            return RedirectToAction("ApproveReports");
        }

        // Xóa báo cáo tiến độ
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _progressReportRepository.GetProgressReportByIdAsync(id);
            if (report == null)
            {
                return Json(new { success = false, message = "Không tìm thấy báo cáo để xóa!" });
            }

            await _progressReportRepository.DeleteProgressReportAsync(id);
            return Json(new { success = true, message = "Báo cáo đã được xóa thành công!" });
        }

        // Xem chi tiết báo cáo tiến độ
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var report = await _progressReportRepository.GetProgressReportByIdAsync(id);
            if (report == null)
            {
                return NotFound("Không tìm thấy báo cáo tiến độ.");
            }

            return View(report);
        }
    }
}
