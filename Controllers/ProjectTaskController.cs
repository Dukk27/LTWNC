using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace WNC_G13.Controllers
{
    public class ProjectTaskController : Controller
    {
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectTaskController> _logger;
        private readonly INotificationRepository _notificationRepository;

        public ProjectTaskController(IProjectTaskRepository taskRepository, 
            IProjectRepository projectRepository, 
            ILogger<ProjectTaskController> logger, 
            INotificationRepository notificationRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _logger = logger;
            _notificationRepository = notificationRepository;
        }

        // Hiển thị danh sách nhiệm vụ cho một dự án cụ thể
        public async Task<IActionResult> Index(int projectId, string searchTerm)
        {
            // Lấy userId và userRole từ Claims
            var userId = User.FindFirstValue("UserId");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var unreadCount = await _notificationRepository.GetUnreadNotificationCountAsync(int.Parse(userId));
            ViewBag.UnreadNotificationCount = unreadCount;

            // Tìm công việc theo từ khóa, nếu không có thì lấy tất cả công việc
            var tasks = await _taskRepository.SearchTasksByProjectIdAsync(projectId, searchTerm);

            // Lấy tên dự án từ repository
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();  // Nếu dự án không tồn tại, trả về lỗi 404
            }

            // Truyền tên dự án vào ViewBag
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = projectId;

            // Truyền từ khóa tìm kiếm vào ViewBag
            ViewBag.SearchTerm = searchTerm;

            // Truyền vai trò người dùng vào ViewBag
            ViewBag.UserRole = userRole;

            // Lấy danh sách người dùng trong dự án và kiểm tra xem ai là PM
            var usersInProject = await _projectRepository.GetUsersInProjectAsync(projectId);

            var usersWithPmStatus = usersInProject.Select(user => new
            {
                user.FullName,
                IsPm = user.UserProjects
                .Where(up => up.ProjectId == projectId)
                .Select(up => up.IsPm)
                .FirstOrDefault()  // Lấy IsPm của người dùng trong dự án cụ thể
            }).ToList();

            ViewBag.UsersInProject = usersWithPmStatus;

            // Cập nhật trạng thái dự án dựa trên trạng thái của các task
            var allCompleted = tasks.All(t => t.Status == (int)Models.TaskStatus.Completed);
            var anyNotCompleted = tasks.Any(t => t.Status != (int)Models.TaskStatus.Completed);

            if (allCompleted)
            {
                if (project.Status != (int)ProjectStatus.ToDo)
                {
                    project.Status = (int)ProjectStatus.ToDo;
                    await _projectRepository.UpdateAsync(project);
                }
            }
            else if (anyNotCompleted)
            {
                if (project.Status != (int)ProjectStatus.NotReady)
                {
                    project.Status = (int)ProjectStatus.NotReady;
                    await _projectRepository.UpdateAsync(project);
                }
            }

            return View(tasks);
        }

        // Hiển thị form tạo nhiệm vụ mới
        [HttpGet]
        public IActionResult Create(int projectId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.ProjectId = projectId;
            return View();
        }

        // Xử lý khi người dùng submit form tạo nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectTask task, int projectId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var projectExists = await _projectRepository.GetByIdAsync(projectId);
            if (projectExists == null)
            {
                ModelState.AddModelError("", "Dự án không tồn tại.");
                return View(task);
            }

            task.ProjectId = projectId;
            await _taskRepository.AddAsync(task);
            return RedirectToAction("Index", new { projectId });
        }

        // Hiển thị form chỉnh sửa nhiệm vụ
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.ProjectId = task.ProjectId;
            return View(task);
        }

        // Xử lý khi người dùng submit form chỉnh sửa nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectTask task)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.ProjectId = task.ProjectId;
            await _taskRepository.UpdateAsync(task);
            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
        }

        // Hiển thị trang xác nhận xóa nhiệm vụ
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.ProjectId = task.ProjectId;
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    return Json(new { success = false, message = "Nhiệm vụ không tồn tại." });
                }

                var projectId = task.ProjectId; // Lưu lại ProjectId để quay lại trang danh sách nhiệm vụ

                await _taskRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xóa nhiệm vụ thành công.", projectId = projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa nhiệm vụ: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa nhiệm vụ." });
            }
        }

        // Hiển thị tất cả các nhiệm vụ
        public async Task<IActionResult> AllTasks()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var tasks = await _taskRepository.GetAllTasksAsync();
            return View(tasks);
        }

        // Cập nhật trạng thái của dự án dựa trên trạng thái của các nhiệm vụ
        [HttpPost]
        public async Task<IActionResult> UpdateProjectStatus(int projectId)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Dự án không tồn tại." });
            }

            var allCompleted = tasks.All(t => t.Status == (int)Models.TaskStatus.Completed);
            var anyNotCompleted = tasks.Any(t => t.Status != (int)Models.TaskStatus.Completed);

            if (allCompleted)
            {
                project.Status = (int)ProjectStatus.ToDo;
            }
            else if (anyNotCompleted)
            {
                project.Status = (int)ProjectStatus.NotReady;
            }

            await _projectRepository.UpdateAsync(project);
            return Json(new { success = true, status = project.Status });
        }

        // Cập nhật trạng thái của nhiệm vụ
        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int status)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return Json(new { success = false, message = "Task không tồn tại." });
            }

            task.Status = status;
            await _taskRepository.UpdateAsync(task);

            return Json(new { success = true });
        }

        // Hiển thị bảng thống kê các nhiệm vụ
        [Authorize]
        public async Task<IActionResult> Board()
        {
            var allTasks = await _taskRepository.GetAllTasksAsync();

            var taskStatusCount = new
            {
                NotReady = allTasks.Count(t => t.Status == (int)Models.TaskStatus.NotReady),
                ToDo = allTasks.Count(t => t.Status == (int)Models.TaskStatus.ToDo),
                InProgress = allTasks.Count(t => t.Status == (int)Models.TaskStatus.InProgress),
                Completed = allTasks.Count(t => t.Status == (int)Models.TaskStatus.Completed),
                Delayed = allTasks.Count(t => t.Status == (int)Models.TaskStatus.Delayed)
            };

            ViewBag.TaskStatusCount = taskStatusCount;

            return View();
        }
    }
}
