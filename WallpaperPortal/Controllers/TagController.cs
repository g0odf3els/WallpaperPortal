using Microsoft.AspNetCore.Mvc;
using WallpaperPortal.Persistance;

namespace WallpaperPortal.Controllers
{
    public class TagController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Tags(string prefix)
        {
            var tags = _unitOfWork.TagRepository.FindAllByCondition(t => t.Name.StartsWith(prefix));
            return Json(tags);
        }
    }
}
