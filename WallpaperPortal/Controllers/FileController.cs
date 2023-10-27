using WallpaperPortal.Models;
using WallpaperPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WallpaperPortal.Persistance;
using ImageMagick;
using WallpaperPortal.Queries;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Dreamscape.Controllers
{
    public class FileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FileController> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public FileController(IUnitOfWork unitOfWork, ILogger<FileController> logger, IHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _hostEnvironment = hostEnvironment;

            foreach (var file in _unitOfWork.FileRepository.FindAll())
            {
                if (System.IO.File.Exists($"wwwroot/{file.Path}") && !System.IO.File.Exists($"wwwroot/{file.PreviewPath}"))
                {
                    CreatePreviewForFile(file);
                }
            }
        }

        [HttpGet("Files")]
        public IActionResult Files([FromQuery] FilesQuery query = null, int page = 1, int pageSize = 16)
        {
            try
            {
                Expression<Func<File, bool>>[] expressions = new Expression<Func<File, bool>>[0];

                if (!string.IsNullOrEmpty(query.Tag))
                {
                    expressions = expressions.Append(f => f.Tags.ToList().Any(t => t.Name == query.Tag)).ToArray();
                }

                var f = Expression.Parameter(typeof(File), "f");

                if (query.Resolutions != null)
                {
                    List<(int, int)> resolutions = ParseResolutions(query.Resolutions);

                    var widthParameter = Expression.PropertyOrField(f, "Width");
                    var heightParameter = Expression.PropertyOrField(f, "Height");

                    Expression finalExpression = null;

                    foreach (var resolution in resolutions)
                    {
                        var widthEquals = Expression.Equal(widthParameter, Expression.Constant(resolution.Item1));
                        var heightEquals = Expression.Equal(heightParameter, Expression.Constant(resolution.Item2));

                        var andExpression = Expression.And(widthEquals, heightEquals);

                        if (finalExpression == null)
                        {
                            finalExpression = andExpression;
                        }
                        else
                        {
                            finalExpression = Expression.Or(finalExpression, andExpression);
                        }
                    }

                    if (finalExpression != null)
                    {
                        var lambda = Expression.Lambda<Func<File, bool>>(finalExpression, f);
                        expressions = expressions.Append(lambda).ToArray();
                    }
                }

                var result = _unitOfWork.FileRepository.GetPaged(page, pageSize, new[] { "Tags" }, expressions);

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
                    var similarFiles = _unitOfWork.FileRepository.FindAllByCondition(f => f.Tags.Any(tag => file.Tags.Contains(tag))).Take(8).ToList();
                    similarFiles.RemoveAll(f => f.Id == file.Id);

                    return View(new FileViewModel()
                    {
                        File = file,
                        SimilarFiles = similarFiles
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

                if (!user.EmailConfirmed && !_hostEnvironment.IsDevelopment())
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
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var fileId = Guid.NewGuid().ToString();

                var filePath = $"/Uploads/Files/{fileId}{Path.GetExtension(upload.FileName)}";

                using (var stream = new FileStream($"wwwroot/{filePath}", FileMode.Create))
                {
                    upload.CopyTo(stream);
                }

                File file = new File()
                {
                    Id = fileId,
                    Name = $"{fileId}{Path.GetExtension(upload.FileName)}",
                    Lenght = upload.Length,
                    Path = $"/Uploads/Files/{fileId}{Path.GetExtension(upload.FileName)}",
                    CreationTime = DateTime.Now,
                    UserId = userId
                };

                using (MagickImage image = new MagickImage($"wwwroot/{filePath}"))
                {
                    file.Height = image.Height;
                    file.Width = image.Width;
                }

                CreatePreviewForFile(file);

                AddTagsToFile(file, tagsList?.Split(',').ToList());

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

                if (System.IO.File.Exists($"wwwroot/{file.Path}"))
                {
                    System.IO.File.Delete($"wwwroot/{file.Path}");
                }

                if (System.IO.File.Exists($"wwwroot/{file.PreviewPath}"))
                {
                    System.IO.File.Delete($"wwwroot/{file.PreviewPath}");
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

        private void CreatePreviewForFile(File file)
        {
            using (MagickImage image = new MagickImage($"wwwroot/{file.Path}"))
            {
                double aspectRatio = (double)image.Width / image.Height;

                int _previewWidth = 450;
                int _previewHeight = (int)(_previewWidth / aspectRatio);

                image.Resize(new MagickGeometry(_previewWidth, _previewHeight)
                {
                    FillArea = true
                });

                image.Extent(_previewWidth, _previewHeight, Gravity.Center);

                if (file.PreviewPath == null)
                {
                    file.PreviewPath = $"/Uploads/Previews/{file.Id}.jpg";
                }

                image.Write($"wwwroot/{file.PreviewPath}");
            }
        }

        private void AddTagsToFile(File file, List<string>? tags)
        {
            if (tags == null)
            {
                return;
            }

            foreach (var tagName in tags)
            {
                var Tag = _unitOfWork.TagRepository.FindFirstByCondition(tag => tag.Name == tagName.ToLower());

                if (Tag == null)
                {
                    Tag = new Tag()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = tagName.ToLower()
                    };
                    _unitOfWork.TagRepository.Create(Tag);
                }

                file.Tags.Add(Tag);
            }
        }

        private static List<(int, int)> ParseResolutions(string input)
        {
            List<(int, int)> resolutions = new List<(int, int)>();
            string pattern = @"(\d+)x(\d+)";

            MatchCollection matches = Regex.Matches(input, pattern);
            foreach (Match match in matches)
            {
                int width = int.Parse(match.Groups[1].Value);
                int height = int.Parse(match.Groups[2].Value);
                resolutions.Add((width, height));
            }

            return resolutions;
        }

    }
}