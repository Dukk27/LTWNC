using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho tất cả các action
    public class OrganizationController : Controller
    {
        private readonly WNCG13Context _context;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;

        public OrganizationController(WNCG13Context context, IOrganizationRepository organizationRepository, IUserRepository userRepository)
        {
            _context = context;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue("UserId"));
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Index()
        {
            var organizations = await _organizationRepository.GetAllAsync();

            ViewBag.UserId = GetUserId();
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);

            return View(organizations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.UserId = GetUserId();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Organization organization)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            organization.CreatedBy = GetUserId();

            var createdOrganization = await _organizationRepository.AddAsync(organization);

            if (createdOrganization != null)
            {
                TempData["SuccessMessage"] = "Tổ chức đã được tạo thành công!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo tổ chức.";
            return View(organization);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Organization organization)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            organization.CreatedBy = GetUserId();

            if (id != organization.Id)
            {
                return NotFound();
            }

            var updatedOrganization = await _organizationRepository.UpdateAsync(organization);

            if (updatedOrganization != null)
            {
                TempData["SuccessMessage"] = "Sửa thành công!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi sửa tổ chức.";
            return View(organization);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa tổ chức." });
            }

            try
            {
                await _organizationRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xóa tổ chức thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tổ chức: " + ex.Message });
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            var organization = await _context.Organizations
                .Include(o => o.UserOrganizations)
                .ThenInclude(uo => uo.User)
                .Include(o => o.Projects)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                return NotFound();
            }

            var users = await _userRepository.GetAllAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName");

            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);
            ViewBag.UserId = GetUserId();

            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int organizationId, int userId)
        {
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (organization == null || user == null)
            {
                TempData["ErrorMessage"] = "Tổ chức hoặc người dùng không tồn tại.";
                return RedirectToAction("Detail", new { id = organizationId });
            }

            var existingUserOrganization = await _context.UserOrganizations
                .FirstOrDefaultAsync(uo => uo.OrganizationId == organizationId && uo.UserId == userId);

            if (existingUserOrganization != null)
            {
                TempData["ErrorMessage"] = "Người dùng đã là thành viên của tổ chức này.";
                return RedirectToAction("Detail", new { id = organizationId });
            }

            var userOrganization = new UserOrganization
            {
                OrganizationId = organizationId,
                UserId = userId
            };

            try
            {
                _context.UserOrganizations.Add(userOrganization);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thành viên đã được thêm vào tổ chức thành công!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm thành viên vào tổ chức.";
            }

            return RedirectToAction("Detail", new { id = organizationId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int organizationId, int userId)
        {
            var userOrganization = await _context.UserOrganizations
                .FirstOrDefaultAsync(uo => uo.OrganizationId == organizationId && uo.UserId == userId);

            if (userOrganization == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thành viên trong tổ chức này.";
                return RedirectToAction("Detail", new { id = organizationId });
            }

            try
            {
                _context.UserOrganizations.Remove(userOrganization);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa thành viên khỏi tổ chức thành công.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thành viên.";
            }

            return RedirectToAction("Detail", new { id = organizationId });
        }
    }
}
