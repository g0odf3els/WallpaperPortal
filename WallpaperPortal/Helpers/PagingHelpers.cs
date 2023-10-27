using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;
using WallpaperPortal.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WallpaperPortal.Helpers
{
    public static class PagingHelpers
    {
        public static IHtmlContent PaginationLinks(this IHtmlHelper html, PagedList<File> model)
        {
            var ulTag = new TagBuilder("ul");
            ulTag.AddCssClass("pagination");

            if (model.TotalPages <= 1)
                return ulTag;

            var request = html.ViewContext.HttpContext.Request;
            var queryString = request.QueryString;

            AddPreviousPageLink(ulTag, model, request, queryString);
            AddPageLinks(ulTag, model, request, queryString);
            AddNextPageLink(ulTag, model, request, queryString);

            return ulTag;
        }

        private static void AddPreviousPageLink(TagBuilder ulTag, PagedList<File> model, HttpRequest request, QueryString queryString)
        {
            if (model.PageNumber != 1)
            {
                var prevLiTag = new TagBuilder("li");
                var prevATag = new TagBuilder("a");
                prevATag.MergeAttribute("href", $"{request.Path}?{ReplacePageValue(queryString, model.PageNumber - 1)}");

                var prevSpanTag = new TagBuilder("span");
                prevSpanTag.AddCssClass("material-symbols-outlined");
                prevSpanTag.InnerHtml.AppendHtml("navigate_before");

                prevATag.InnerHtml.AppendHtml(prevSpanTag);
                prevLiTag.InnerHtml.AppendHtml(prevATag);

                ulTag.InnerHtml.AppendHtml(prevLiTag);
            }
        }

        private static void AddPageLinks(TagBuilder ulTag, PagedList<File> model, HttpRequest request, QueryString queryString)
        {
            for (var i = Math.Max(1, model.PageNumber - 2); i <= Math.Min(model.TotalPages, model.PageNumber + 2); i++)
            {
                var liTag = new TagBuilder("li");

                var aTag = new TagBuilder("a");
                aTag.InnerHtml.AppendHtml(i.ToString());

                liTag.InnerHtml.AppendHtml(aTag);

                if (i == model.PageNumber)
                {
                    liTag.AddCssClass("active");
                }
                else
                {
                    aTag.MergeAttribute("href", $"{request.Path}?{ReplacePageValue(queryString, i)}");
                }

                ulTag.InnerHtml.AppendHtml(liTag);
            }
        }

        private static void AddNextPageLink(TagBuilder ulTag, PagedList<File> model, HttpRequest request, QueryString queryString)
        {
            if (model.PageNumber != model.TotalPages)
            {
                var nextLiTag = new TagBuilder("li");
                var nextATag = new TagBuilder("a");
                nextATag.MergeAttribute("href", $"{request.Path}?{ReplacePageValue(queryString, model.PageNumber + 1)}");
                var nextSpanTag = new TagBuilder("span");
                nextSpanTag.AddCssClass("material-symbols-outlined");
                nextSpanTag.InnerHtml.AppendHtml("navigate_next");

                nextATag.InnerHtml.AppendHtml(nextSpanTag);
                nextLiTag.InnerHtml.AppendHtml(nextATag);

                ulTag.InnerHtml.AppendHtml(nextLiTag);
            }
        }

        private static string ReplacePageValue(QueryString input, int newValue)
        {
            var queryString = HttpUtility.ParseQueryString(input.ToString());

            int currentPage = -1;
            if (!string.IsNullOrEmpty(queryString["page"]))
            {
                if (int.TryParse(queryString["page"], out int parsedValue))
                {
                    currentPage = parsedValue;
                }
            }

            if (currentPage != newValue)
            {
                queryString.Set("page", newValue.ToString());
            }

            return queryString.ToString();
        }
    }
}
