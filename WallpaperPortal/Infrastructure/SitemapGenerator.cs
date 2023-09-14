using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Xml.Linq;
using WallpaperPortal.Models;
using WallpaperPortal.Persistance;
using WallpaperPortal.Queries;

namespace WallpaperPortal.Infrastructure
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly IUnitOfWork _unitOfWork;

        public SitemapGenerator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyCollection<string> GetSitemapNodes(IUrlHelper urlHelper)
        {
            List<string> nodes = new List<string>();

            nodes.Add(urlHelper.AbsoluteAction("Login", "Authorization"));
            nodes.Add(urlHelper.AbsoluteAction("Register", "Authorization"));

            nodes.Add(urlHelper.AbsoluteAction("Files", "File"));

            foreach (var file in _unitOfWork.FileRepository.FindAll())
            {
                nodes.Add(urlHelper.AbsoluteAction("File", "File", new { id = file.Id }));
            }

            foreach (var tag in _unitOfWork.TagRepository.FindAll())
            {
                nodes.Add(urlHelper.AbsoluteAction("Files", "File", new { Tag = tag.Name }));
            }

            return nodes;
        }

        public string GetSitemapDocument(IEnumerable<string> sitemapNodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");

            foreach (string sitemapNode in sitemapNodes)
            {
                XElement urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", sitemapNode));
                root.Add(urlElement);
            }

            XDocument document = new XDocument(root);
            return document.ToString();
        }
    }
    public static class UrlHelperExtensions
    {
        public static string AbsoluteAction(
             this IUrlHelper url,
             string actionName,
             string controllerName,
             object routeValues = null)
        {
            string scheme = url.ActionContext.HttpContext.Request.Scheme;
            return url.Action(actionName, controllerName, routeValues, scheme);
        }
    }
}