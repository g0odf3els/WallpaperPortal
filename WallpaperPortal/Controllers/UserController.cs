using WallpaperPortal.Models;
using WallpaperPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WallpaperPortal.Persistance;
using System.Security.Claims;
using ImageMagick;

namespace WallpaperPortal.Controllers
{
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<User> userManager, IUnitOfWork unitOfWork, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View(_unitOfWork.UserRepository.FindAll());
            }
            else
            {
                return View(_unitOfWork.UserRepository.FindAll(user => user.UserName.Contains(username)));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    Email = model.Email,
                    UserName = model.Username
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Username;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Profile(string id, int page = 1, int pageSize = 16)
        {
            var user = _unitOfWork.UserRepository.FindFirst(u => u.Id == id);

            if (user is null)
            {
                return NotFound();
            }

            var pagedList = _unitOfWork.FileRepository.GetPaged(page, pageSize, new[] { "Tags" }, f => f.UserId == user.Id);

            return View(new ProfileViewModel()
            {
                User = user,
                UploadedFiles = pagedList
            });
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangeProfileImage()
        {
            var user = _unitOfWork.UserRepository.FindFirst(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeProfileImage(IFormFile upload)
        {

            var user = _unitOfWork.UserRepository.FindFirst(user => user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null)
            {
                throw new Exception();
            }

            using (MagickImage image = new MagickImage(upload.OpenReadStream()))
            {

                int targetWidth = 240;
                int targetHeight = 320;

                image.Resize(new MagickGeometry(targetWidth, targetHeight)
                {
                    FillArea = true
                });

                image.Extent(targetWidth, targetHeight, Gravity.Center);

                image.Write($"wwwroot/Uploads/ProfileImages/{user.Id}{Path.GetExtension(upload.FileName)}");

                if (System.IO.File.Exists($"wwwroot/{user.ProfileImage}"))
                {
                    System.IO.File.Delete($"wwwroot/{user.ProfileImage}");
                }

            }

            user.ProfileImage = $"/Uploads/ProfileImages/{user.Id}{Path.GetExtension(upload.FileName)}";

            _unitOfWork.Save();
            return RedirectToAction("Profile", "Users", new { id = user.Id });
        }
    }
}