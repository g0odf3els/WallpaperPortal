using WallpaperPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WallpaperPortal.Persistance;
using WallpaperPortal.Queries;
using WallpaperPortal.Services.Abstract;
using Microsoft.AspNetCore.Identity;

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

        [Authorize]
        public IActionResult Relevant([FromQuery] FilesQuery query)
        {
            var user = _unitOfWork.UserRepository.FindFirst(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier), new[] {"LikedTags"} );
            query.Tags = user.LikedTags.Select(t => t.Name).ToArray();

            return View("Files", new FilesViewModel()
            {
                PagedList = _fileService.Files(query ),
                FilesQuery = query
            });
        }

        [HttpGet("File")]
        public IActionResult File(string id)
        {
            var file = _fileService.File(id);
            var user = _unitOfWork.UserRepository.FindFirst(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            return file == null
                ? NotFound()
                : View(new FileViewModel()
                {
                    File = file,
                    SimilarFiles = _fileService.SimilarFiles(file),
                    isFavorite = user != null && file.LikedByUsers.Any(f => f.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
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

        [Authorize]
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

        [Authorize]
        [HttpPost("RemoveTag")]
        public IActionResult RemoveTag(string id, string tagName)
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

        [Authorize]
        public IActionResult AddFileToFavorites(string id)
        {
            _fileService.AddFileToFavorite(User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            return RedirectToAction("File", "File", new { id });
        }

        [Authorize]
        public IActionResult RemoveFileToFavorites(string id)
        {
            _fileService.RemoveFileFromFavorite(User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            return RedirectToAction("File", "File", new { id });
        }
    }
}