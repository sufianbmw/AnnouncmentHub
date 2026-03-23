using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace AnnouncmentHub.Areas.Admin.Controllers
{
    //[Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class CkEditorController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadRoot;

        public CkEditorController(IWebHostEnvironment env)
        {
            _env = env;
            _uploadRoot = Path.Combine(_env.WebRootPath, "uploads/ckeditor");
            if (!Directory.Exists(_uploadRoot))
                Directory.CreateDirectory(_uploadRoot);
        }

        // 📤 Upload file (used by CKEditor dialog)
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return Json(new { uploaded = 0, error = new { message = "لم يتم تحميل أي ملف." } });

            string ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
            string[] allowed = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".rar"];

            if (!allowed.Contains(ext))
                return Json(new { uploaded = 0, error = new { message = "نوع الملف غير مدعوم." } });

            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}-{Path.GetFileName(upload.FileName)}";
            string filePath = Path.Combine(_uploadRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            string url = Url.Content($"~/uploads/ckeditor/{fileName}");
            return Json(new { uploaded = 1, fileName, url });
        }

        // 📁 File browser popup view
        [HttpGet]
        public IActionResult Browse()
        {
            var dir = new DirectoryInfo(_uploadRoot);
            ViewBag.fileInfos = dir.GetFiles();
            return View("~/areas/admin/Views/CkEditor/Browse.cshtml");
        }

        [HttpDelete]
        [HttpPost] // allow POST too
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Json(new { success = false, message = "اسم الملف غير صالح." });

            var path = Path.Combine(_uploadRoot, fileName);
            if (!System.IO.File.Exists(path))
                return Json(new { success = false, message = "الملف غير موجود." });

            System.IO.File.Delete(path);
            return Json(new { success = true, message = "تم حذف الملف بنجاح." });
        }


        // 🧾 Optional JSON list for dynamic browsers
        [HttpGet]
        public IActionResult ListFiles()
        {
            var files = Directory.GetFiles(_uploadRoot)
                .Select(f => new { name = Path.GetFileName(f), url = Url.Content($"~/uploads/ckeditor/{Path.GetFileName(f)}") });
            return Json(files);
        }
    }
}
