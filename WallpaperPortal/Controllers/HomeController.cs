using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;
using WallpaperPortal.Infrastructure;
using WallpaperPortal.Models;

namespace WallpaperPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISitemapGenerator _sitemapGenerator;

        public HomeController(ILogger<HomeController> logger, ISitemapGenerator sitemapGenerator)
        {
            _logger = logger;
            _sitemapGenerator = sitemapGenerator;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Sitemap()
        {
            var sitemapNodes = _sitemapGenerator.GetSitemapNodes(this.Url);
            string xml = _sitemapGenerator.GetSitemapDocument(sitemapNodes);
            return this.Content(xml, "text/xml", Encoding.UTF8);
        }
    }
}