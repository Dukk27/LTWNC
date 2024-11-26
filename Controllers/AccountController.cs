using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WNC_G13.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            try
            {
                // Kiểm tra email đã tồn tại chưa
                var isRegistered = await _userRepository.RegisterUserAsync(model);

                if (isRegistered)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công";
                    // Đăng ký thành công, chuyển hướng đến trang login
                    return RedirectToAction("Login", "Account");
                }

                // Nếu email đã tồn tại, thêm lỗi vào ModelState
                ModelState.AddModelError("", "Email đã tồn tại. Vui lòng thử lại.");
            }
            catch (DbUpdateException)
            {
                // Bắt lỗi DB khi không thể insert dữ liệu (chẳng hạn lỗi NULL vào cột)
                ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký. Vui lòng kiểm tra lại các trường thông tin.");
            }

            // Trả về lại view với thông báo lỗi
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userRepository.LoginUserAsync(email, password);
            if (user != null)
            {
                // Tạo danh sách claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role == Role.AdminRole ? "Admin" : "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Đăng nhập với Claims
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            // Kiểm tra quyền Admin
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var users = await _userRepository.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Chỉ Admin được phép chỉnh sửa
            // if (!User.IsInRole("Admin"))
            // {
            //     return RedirectToAction("AccessDenied", "Home");
            // }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            // Chỉ Admin được phép chỉnh sửa
            // if (!User.IsInRole("Admin"))
            // {
            //     return RedirectToAction("AccessDenied", "Home");
            // }

            if (string.IsNullOrEmpty(user.Password))
            {
                var existingUser = await _userRepository.GetUserByIdAsync(user.Id);
                if (existingUser != null)
                {
                    user.Password = existingUser.Password; // Giữ mật khẩu cũ
                }
            }

            await _userRepository.UpdateUserAsync(user);

            var refererUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(refererUrl))
            {
                return Redirect(refererUrl);
            }

            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userRepository.DeleteUserAsync(id);
                return Json(new { success = true, message = "Xoá thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thực hiện xoá: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = User.FindFirstValue("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirstValue("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.Password != currentPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không chính xác.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            var isUpdated = await _userRepository.UpdatePasswordAsync(user.Id, newPassword);
            if (isUpdated)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi thay đổi mật khẩu.");
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize] // Đảm bảo người dùng đã đăng nhập
        public IActionResult Setting()
        {
            // Lấy userId từ Claims
            var userId = User.FindFirstValue("UserId"); // Claims lưu thông tin người dùng

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userRepository.GetUserByIdAsync(int.Parse(userId)).Result; // Chuyển đổi UserId từ string sang int
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Trả về view cài đặt người dùng với dữ liệu người dùng
        }

        // Cập nhật thông tin người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Đảm bảo người dùng đã đăng nhập
        public async Task<IActionResult> UpdateSetting(User model)
        {
            // Lấy userId từ Claims
            var userId = User.FindFirstValue("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                await _userRepository.UpdateUserAsync(model); // Cập nhật thông tin người dùng vào cơ sở dữ liệu
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!"; // Hiển thị thông báo thành công
                return RedirectToAction("Setting"); // Quay lại trang cài đặt
            }

            return View("Setting", model); // Nếu dữ liệu không hợp lệ, trả lại view cài đặt với lỗi
        }
    }
}

