using WallpaperPortal.Models;

namespace WallpaperPortal.Queries
{
	public class FilesQuery
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 16;

		public string[]? Tags { get; set; } = new string[] { };

		public string? Width { get; set; }
		public string? Height { get; set; }

		public string? Resolutions { get; set; }
		public string? AspectRatios { get; set; }
    }
}
