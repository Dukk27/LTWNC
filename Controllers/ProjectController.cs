using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    [Authorize] // Đảm bảo người dùng phải được xác thực
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IProgressReportRepository _progressReportRepository;

        public ProjectController(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            INotificationRepository notificationRepository,
            IProgressReportRepository progressReportRepository
        )
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _notificationRepository = notificationRepository;
            _progressReportRepository = progressReportRepository;
        }
        

        public async Task<IActionResult> Index(int organizationId)
        {
            var userId = User.FindFirstValue("UserId");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var unreadCount = await _notificationRepository.GetUnreadNotificationCountAsync(int.Parse(userId));
            ViewBag.UnreadNotificationCount = unreadCount;

            var unreadReportsCount = await _progressReportRepository.GetUnreadReportsCountForAdminAsync();
            ViewBag.UnreadReportsCount = unreadReportsCount;

            // Kiểm tra nếu người dùng là Admin, cho phép xem tất cả dự án
            if (userRole == "Admin")
            {
                var projects = await _projectRepository.GetProjectsByOrganizationIdAsync(organizationId);
                ViewBag.OrganizationId = organizationId;
                ViewBag.UserRole = userRole;

                var organizations = await _organizationRepository.GetAllAsync();
                ViewBag.Organizations = new SelectList(organizations, "Id", "Name");

                return View(projects);
            }

            // Kiểm tra xem người dùng có thuộc tổ chức này không
            var userOrganizations = await _userRepository.GetOrganizationsByUserIdAsync(int.Parse(userId));
            if (!userOrganizations.Contains(organizationId))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Nếu người dùng thuộc tổ chức, lấy danh sách dự án của tổ chức đó
            var projectsForUser = await _projectRepository.GetProjectsByOrganizationIdAsync(organizationId);
            ViewBag.OrganizationId = organizationId;
            ViewBag.UserRole = userRole;

            var userOrganizationsList = await _organizationRepository.GetAllAsync();
            var organizationsForUser = userOrganizationsList.Where(o => userOrganizations.Contains(o.Id)).ToList();
            ViewBag.Organizations = new SelectList(organizationsForUser, "Id", "Name");

            var usersInOrganizationForUser = await _userRepository.GetUsersByOrganizationIdAsync(organizationId);
            ViewBag.UsersInOrganization = new SelectList(usersInOrganizationForUser, "Id", "FullName");

            return View(projectsForUser);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var usersInProject = await _projectRepository.GetUsersInProjectAsync(id);
            var userProjects = await _projectRepository.GetUserProjectsByProjectIdAsync(id);

            ViewBag.UsersInProject = usersInProject ?? new List<User>();
            ViewBag.UserProjects = userProjects ?? new List<UserProject>();
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);

            var allUsers = await _userRepository.GetAllAsync();
            ViewBag.AllUsers = allUsers;

            return View(project);
        }

        [HttpGet]
        public IActionResult Create(int organizationId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.OrganizationId = organizationId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, int organizationId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            project.CreatedBy = int.Parse(User.FindFirstValue("UserId"));
            project.CreatedAt = DateTime.Now;
            project.OrganizationId = organizationId;

            await _projectRepository.AddAsync(project);
            return RedirectToAction(nameof(Index), new { organizationId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.OrganizationId = project.OrganizationId;
            ViewBag.UserId = User.FindFirstValue("UserId");
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Project project)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (!project.OrganizationId.HasValue)
            {
                return BadRequest("OrganizationId is required.");
            }

            await _projectRepository.UpdateAsync(project);
            return RedirectToAction(nameof(Index), new { organizationId = project.OrganizationId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            try
            {
                await _projectRepository.DeleteAsync(id);
                return Json(new { success = true, projectId = id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa dự án không thành công: " + ex.Message });
            }
        }

        // Hiển thị tất cả các dự án (chỉ dành cho Admin)
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AllProjects()
        {
            // Lấy tất cả các dự án từ repository
            var projects = await _projectRepository.GetAllProjectsAsync();

            // Trả về view với danh sách các dự án
            return View(projects);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AddUserToProject(int projectId, int userId, bool isPm)
        {
            try
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (userRole != "Admin")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này.";
                    return RedirectToAction("Detail", new { id = projectId });
                }

                await _projectRepository.AddUserToProjectAsync(projectId, userId, isPm);
                TempData["SuccessMessage"] = "Thêm người dùng vào dự án thành công!";
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                                                sqlEx.Number == 2627)
            {
                TempData["ErrorMessage"] = "Người dùng đã thuộc dự án này.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm người dùng vào dự án.";
            }

            return RedirectToAction("Detail", new { id = projectId });
        }
    }
}
