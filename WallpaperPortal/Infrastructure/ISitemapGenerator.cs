using Microsoft.AspNetCore.Mvc;

namespace WallpaperPortal.Infrastructure
{
    public interface ISitemapGenerator
    {
        string GetSitemapDocument(IEnumerable<string> sitemapNodes);
        public IReadOnlyCollection<string> GetSitemapNodes(IUrlHelper urlHelper);
    }
}