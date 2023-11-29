using WallpaperPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WallpaperPortal.Persistance;
using WallpaperPortal.Queries;
using WallpaperPortal.Services.Abstract;

namespace Dreamscape.Controllers
{
    public class FileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public FileController(IUnitOfWork unitOfWork, IFileService fileService, ILogger<FileController> logger, IHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Files([FromQuery] FilesQuery query)
        {
            return View(new FilesViewModel()
            {
                PagedList = _fileService.Files(query),
                FilesQuery = query
            });
        }

        [HttpGet("File")]
        public IActionResult File(string id)
        {
            var file = _fileService.File(id);

            return file == null
                ? NotFound()
                : View(new FileViewModel()
                {
                    File = file,
                    SimilarFiles = _fileService.SimilarFiles(file)
                });

        }


        [Authorize]
        [HttpGet("Upload")]
        public IActionResult Upload()
        {
            var user = _unitOfWork.UserRepository.FindFirst(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null)
            {
                return BadRequest();
            }

            if (!user.EmailConfirmed && !_hostEnvironment.IsDevelopment())
            {
                return Forbid();
            }

            return View();
        }

        [Authorize]
        [HttpPost("Upload")]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile[] uploads, string? tagsList = null)
        {
            if (uploads == null)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return BadRequest();
            }

            _fileService.Upload(uploads, userId, tagsList?.Split(','));

            return RedirectToAction("Files", "File");
        }

        [Authorize]
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {

            var file = _fileService.File(id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _fileService.Delete(file);

            return RedirectToAction("Files", "File");
        }

        [HttpPost("Download")]
        public IActionResult Download(string id)
        {
            var file = _fileService.File(id);

            return file == null ? NotFound() : File(file.Path, "text/plain", file.Name);
        }

        [HttpPost("AddTag")]
        public IActionResult AddTag(string id, string tagName)
        {

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tagName))
            {
                return BadRequest();
            }

            var file = _fileService.File(id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _fileService.AddTagsToFile(file, new[] { tagName });

            return RedirectToAction("File", "File", new { id });
        }

        [HttpPost("RemoveTag")]
        public async Task<IActionResult> RemoveTag(string id, string tagName)
        {

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tagName))
            {
                return BadRequest();
            }

            var file = _fileService.File(id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _fileService.RemoveTagsFromFile(file, new[] { tagName });

            return RedirectToAction("File", "File", new { id });
        }

        public IActionResult Resize(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var file = _unitOfWork.FileRepository.FindFirst(file => file.Id == id);

            return file == null ? NotFound() : View(file);
        }
    }
}