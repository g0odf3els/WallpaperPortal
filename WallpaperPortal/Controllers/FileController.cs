using WallpaperPortal.Models;
using WallpaperPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WallpaperPortal.Persistance;
using ImageMagick;
using WallpaperPortal.Queries;
using System.Linq.Expressions;

namespace Dreamscape.Controllers
{
    public class FileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FileController> _logger;

        public FileController(IUnitOfWork unitOfWork, ILogger<FileController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("Files")]
        public IActionResult Files([FromQuery] FilesQuery query = null, int page = 1, int pageSize = 16)
        {
            try
            {
                Expression<Func<File, bool>>[] expressions = new Expression<Func<File, bool>>[0];

                if (!string.IsNullOrEmpty(query.Tag))
                {
                    expressions = expressions.Append(f => f.Tags.Any(t => t.Name == query.Tag)).ToArray();
                }

                if (!string.IsNullOrEmpty(query.Width))
                {
                    expressions = expressions.Append(f => f.Width.ToString() == query.Width).ToArray();
                }

                if (!string.IsNullOrEmpty(query.Height))
                {
                    expressions = expressions.Append(f => f.Height.ToString() == query.Height).ToArray();
                }

                var result = _unitOfWork.FileRepository.GetPaged(page, pageSize, expressions);

                if (result != null)
                {
                    return View(new FilesViewModel
                    {
                        PagedList = result,
                        FilesQuery = query,
                    });
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet("File")]
        public IActionResult File(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var file = _unitOfWork.FileRepository.FindFirstByCondition(file => file.Id == id, new[] { "User", "Tags" });

                if (file != null)
                {
                    return View(file);
                }

                return NotFound();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }


        [Authorize]
        [HttpGet("Upload")]
        public IActionResult Upload()
        {
            try
            {
                var user = _unitOfWork.UserRepository.FindFirstByCondition(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (user == null)
                {
                    return BadRequest();
                }

                if (!user.EmailConfirmed)
                {
                    return Forbid();
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }

        }

        [Authorize]
        [HttpPost("Upload")]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile upload, string tagsList)
        {
            try
            {
                int _previewWidth = 432;
                int _previewHeight = 243;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string fileId = Guid.NewGuid().ToString();

                var filePath = $"/Uploads/Files/{fileId}{Path.GetExtension(upload.FileName)}";
                var previewPath = $"/Uploads/Previews/{fileId}.jpg";

                using (var stream = new FileStream($"wwwroot/{filePath}", FileMode.Create))
                {
                    upload.CopyTo(stream);
                }

                File file;

                using (MagickImage image = new MagickImage($"wwwroot/{filePath}"))
                {
                    file = new File()
                    {
                        Id = fileId,
                        Name = fileId,
                        Height = image.Height,
                        Width = image.Width,
                        Lenght = upload.Length,
                        Path = filePath,
                        PreviewPath = previewPath,
                        CreationTime = DateTime.Now,
                        UserId = userId
                    };

                    image.Resize(new MagickGeometry(_previewWidth, _previewHeight)
                    {
                        FillArea = true
                    });

                    image.Extent(_previewWidth, _previewHeight, Gravity.Center);

                    image.Write($"wwwroot/{previewPath}");
                }

                List<string> fileTags = tagsList?.Split(',').ToList() ?? new List<string>();
                foreach (var tagName in fileTags)
                {
                    var Tag = _unitOfWork.TagRepository.FindFirstByCondition(tag => tag.Name == tagName);
                    if (Tag == null)
                    {
                        Tag = new Tag()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = tagName
                        };
                        _unitOfWork.TagRepository.Create(Tag);
                    }
                    file.Tags.Add(Tag);
                }
                _unitOfWork.FileRepository.Create(file);
                _unitOfWork.Save();

                return RedirectToAction("Files", "File");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var file = _unitOfWork.FileRepository.FindFirstByCondition(f => f.Id == id);

                if (file == null)
                {
                    return NotFound();
                }

                if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _unitOfWork.FileRepository.Delete(file);
                _unitOfWork.Save();

                return RedirectToAction("Files", "File");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("Download")]
        public IActionResult Download(string id)
        {
            try
            {
                var file = _unitOfWork.FileRepository.FindFirstByCondition(f => f.Id == id);

                if (file != null)
                {
                    return File(file.Path, "text/plain", file.Name);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("AddTag")]
        public async Task<IActionResult> AddTag(string id, string tagName)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tagName))
                {
                    return BadRequest();
                }

                var file = _unitOfWork.FileRepository.FindFirstByCondition(f => f.Id == id);

                if (file == null)
                {
                    return NotFound();
                }

                if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var tag = _unitOfWork.TagRepository.FindFirstByCondition(t => t.Name == tagName);

                if (tag == null)
                {
                    string tagId = Guid.NewGuid().ToString();

                    tag = new Tag()
                    {
                        Id = tagId,
                        Name = tagName
                    };
                    _unitOfWork.TagRepository.Create(tag);
                }

                file.Tags.Add(tag);
                _unitOfWork.Save();

                return RedirectToAction("File", "File", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("RemoveTag")]
        public async Task<IActionResult> RemoveTag(string id, string tagName)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tagName))
                {
                    return BadRequest();
                }

                var file = _unitOfWork.FileRepository.FindFirstByCondition(f => f.Id == id, new[] { "Tags" });

                if (file == null)
                {
                    return NotFound();
                }

                if (file.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !@User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var tag = _unitOfWork.TagRepository.FindFirstByCondition(t => t.Name == tagName);
                file.Tags.Remove(tag);

                _unitOfWork.Save();

                return RedirectToAction("File", "File", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

    }
}