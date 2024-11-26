using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WNC_G13.Models;
using WNC_G13.Repositories;
using System.Security.Claims;

namespace WNC_G13.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly INotificationRepository _notificationRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            INotificationRepository notificationRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy userId và userRole từ Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account"); // Điều hướng đến trang Login nếu không tìm thấy userId
            }

            int userId = int.Parse(userIdClaim.Value); // Lấy userId từ Claims
            string userRole = userRoleClaim?.Value;

            var unreadCount = await _notificationRepository.GetUnreadNotificationCountAsync(userId);
            ViewBag.UnreadNotificationsCount = unreadCount;

            // Nếu là Admin, cho phép truy cập tất cả các dự án
            if (userRole == "Admin")
            {
                var projects = await _projectRepository.GetAllProjectsAsync();
                return View(projects);
            }

            // Nếu không phải Admin, lấy các tổ chức của người dùng và chuyển hướng đến dự án đầu tiên trong tổ chức đó
            var userOrganizations = await _userRepository.GetOrganizationsByUserIdAsync(userId);

            // Nếu người dùng tham gia vào ít nhất 1 tổ chức, lấy dự án thuộc tổ chức đó
            if (userOrganizations.Any())
            {
                var firstOrganizationId = userOrganizations.First(); // Chọn tổ chức đầu tiên
                var projects = await _projectRepository.GetProjectsByOrganizationIdAsync(firstOrganizationId);

                // Nếu có dự án, chuyển hướng đến dự án đầu tiên
                if (projects.Any())
                {
                    return RedirectToAction("Index", "Project", new { organizationId = firstOrganizationId });
                }
            }

            // Nếu không có tổ chức hoặc không có dự án, chuyển đến trang AccessDenied
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> UserProfile()
        {
            // Lấy thông tin người dùng từ Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim == null)
            {
                return RedirectToAction("Index", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Lấy thông tin người dùng từ repository
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(user);
        }

        public IActionResult Help()
        {
            var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userRoleClaim != null && userRoleClaim.Value == "Admin")
            {
                return View("HelpAdmin"); // Trả về view dành cho Admin
            }
            else
            {
                return View("HelpUser"); // Trả về view dành cho User
            }
        }
    }
}
