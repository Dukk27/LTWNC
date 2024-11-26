using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IProjectRepository _projectRepository;

        public DocumentController(IDocumentRepository documentRepository, IProjectRepository projectRepository)
        {
            _documentRepository = documentRepository;
            _projectRepository = projectRepository;
        }

        // Hiển thị danh sách tài liệu của dự án
        public async Task<IActionResult> Index(int projectId)
        {
            var documents = await _documentRepository.GetAllDocumentsAsync(); // Đảm bảo hàm này trả về dữ liệu.
            return View(documents);
        }

        // Tải tài liệu xuống
        public IActionResult Download(int documentId)
        {
            var document = _documentRepository.GetDocumentByIdAsync(documentId).Result;
            if (document == null)
            {
                return NotFound();
            }

            var filePath = document.DocumentUrl;

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", document.DocumentName);
        }

        [HttpGet]
        public IActionResult Upload(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        // Phương thức xử lý khi người dùng upload document
        [HttpPost]
        public async Task<IActionResult> Upload(int projectId, IFormFile documentFile, string description)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin" && userRole != "User")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (documentFile != null && documentFile.Length > 0)
            {
                // Thực hiện lưu trữ file và lưu vào cơ sở dữ liệu
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", documentFile.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(stream);
                }

                // Lưu thông tin document vào cơ sở dữ liệu (bạn có thể thay đổi theo mô hình dữ liệu của mình)
                var document = new Document
                {
                    ProjectId = projectId,
                    DocumentName = documentFile.FileName,
                    DocumentUrl = filePath,
                    UploadedBy = int.Parse(User.FindFirstValue("UserId")),  // Thay bằng UserId từ Claims
                    UploadedAt = DateTime.Now,
                    Description = description
                };

                // Lưu document vào cơ sở dữ liệu
                await _documentRepository.AddDocumentAsync(document); // Sử dụng phương thức để lưu vào cơ sở dữ liệu

                TempData["SuccessMessage"] = "Tải lên tài liệu thành công!";
                var project = await _projectRepository.GetByIdAsync(projectId);
                if (project != null)
                {
                    return RedirectToAction("Index", "Project", new { organizationId = project.OrganizationId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Dự án không tồn tại.";
                    return RedirectToAction("Index", "Project");
                }
            }

            TempData["ErrorMessage"] = "Vui lòng chọn tài liệu để tải lên!";
            return RedirectToAction("Upload", new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int documentId, int projectId)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                return Json(new { success = false, message = "Tài liệu không tồn tại." });
            }

            // Xóa tài liệu khỏi cơ sở dữ liệu
            await _documentRepository.DeleteDocumentAsync(documentId);

            // Xóa file tài liệu khỏi thư mục
            if (System.IO.File.Exists(document.DocumentUrl))
            {
                System.IO.File.Delete(document.DocumentUrl);
            }

            return Json(new { success = true, message = "Tài liệu đã được xóa thành công." });
        }


    }
}
