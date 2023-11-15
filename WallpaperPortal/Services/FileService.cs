using ImageMagick;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using WallpaperPortal.Models;
using WallpaperPortal.Persistance;
using WallpaperPortal.Queries;
using WallpaperPortal.Repositories;
using WallpaperPortal.Services.Abstract;
using Color = WallpaperPortal.Models.Color;

namespace WallpaperPortal.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedList<File> Files(FilesQuery query)
        {
            Expression<Func<File, bool>>[] expressions = new Expression<Func<File, bool>>[0];

            if (!string.IsNullOrEmpty(query.Tag))
            {
                AppendTagFilterExpression(ref expressions, query.Tag);
            }

            if (query.Resolutions != null)
            {
                AppendResolutionFilterExpression(ref expressions, ParsePairs(query.Resolutions));
            }

            if (query.AspectRatios != null)
            {
                AppendAspectRatioFilterExpression(ref expressions, ParsePairs(query.AspectRatios));
            }

            return _unitOfWork.FileRepository.GetPaged(query.Page, query.PageSize, new[] { "Tags" }, expressions);
        }

        private static void AppendTagFilterExpression(ref Expression<Func<File, bool>>[] expressions, string tag)
        {
            expressions = expressions.Append(f => f.Tags.ToList().Any(t => t.Name == tag)).ToArray();
        }

        private static void AppendResolutionFilterExpression(ref Expression<Func<File, bool>>[] expressions, List<(int, int)> resolutions)
        {
            var f = Expression.Parameter(typeof(File), "f");

            var widthParameter = Expression.PropertyOrField(f, "Width");
            var heightParameter = Expression.PropertyOrField(f, "Height");

            Expression? finalExpression = null;

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

        private static void AppendAspectRatioFilterExpression(ref Expression<Func<File, bool>>[] expressions, List<(int, int)> aspectRatios)
        {
            var f = Expression.Parameter(typeof(File), "f");

            Expression? finalExpression = null;

            var widthExpression = Expression.PropertyOrField(f, "Width");
            var heightExpression = Expression.PropertyOrField(f, "Height");

            foreach (var aspect in aspectRatios)
            {

                var aspectEquals = Expression.Equal(
                              Expression.Divide(
                                  Expression.Convert(widthExpression, typeof(double)),
                                  Expression.Convert(heightExpression, typeof(double))),
                              Expression.Constant((double)aspect.Item1 / (double)aspect.Item2));


                finalExpression = finalExpression == null ? aspectEquals : Expression.OrElse(finalExpression, aspectEquals);
            }

            if (finalExpression != null)
            {
                var lambda = Expression.Lambda<Func<File, bool>>(finalExpression, f);
                expressions = expressions.Append(lambda).ToArray();
            }
        }

        private static List<(int, int)> ParsePairs(string input)
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

        public File? File(string id)
        {
            var file = _unitOfWork.FileRepository.FindFirstByCondition(
            file => file.Id == id,
            new[]
            {
                    "User",
                    "Tags",
                    "Colors"
            });

            if(file?.Palette.Count == 0)
            {
                CreatePallet(file);
                _unitOfWork.Save();
            }

            return file;
        }



        public List<File> SimilarFiles(File file)
        {
            var similarFiles = _unitOfWork.FileRepository.FindAllByCondition(f => f.Tags.Any(tag => file.Tags.Contains(tag))).Take(8).ToList();
            similarFiles.RemoveAll(f => f.Id == file.Id);

            return similarFiles;
        }

        public void Upload(IFormFile upload, string userId, string[]? tags)
        {
            var id = Guid.NewGuid().ToString();

            var filePath = $"/Uploads/Files/{id}{Path.GetExtension(upload.FileName)}";

            using (var stream = new FileStream($"wwwroot/{filePath}", FileMode.Create))
            {
                upload.CopyTo(stream);
            }

            File file = new File()
            {
                Id = id,
                Name = $"{id}{Path.GetExtension(upload.FileName)}",
                Lenght = upload.Length,
                Path = $"/Uploads/Files/{id}{Path.GetExtension(upload.FileName)}",
                CreationTime = DateTime.Now,
                UserId = userId
            };

            using (MagickImage image = new MagickImage($"wwwroot/{filePath}"))
            {
                file.Height = image.Height;
                file.Width = image.Width;
            }

            CreatePreviewForFile(file);

            if (tags != null)
            {
                AddTagsToFile(file, tags);
            }

            CreatePallet(file);
         
            _unitOfWork.FileRepository.Create(file);

            _unitOfWork.Save();

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

        public void AddTagsToFile(File file, string[] tags)
        {
            foreach (var tagName in tags)
            {
                var tag = _unitOfWork.TagRepository.FindFirstByCondition(tag => tag.Name == tagName.ToLower())
                    ?? _unitOfWork.TagRepository.Create(new Tag()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = tagName.ToLower()
                    });

                file.Tags.Add(tag);
            }
            _unitOfWork.Save();
        }

        public void RemoveTagsFromFile(File file, string[] tags)
        {
            file.Tags.RemoveAll(t => tags.Contains(t.Name));
            _unitOfWork.Save();
        }

        public void Delete(File file)
        {
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
        }

        public void CreatePallet(File file)
        {
            foreach (var color in ColorUtils.GetColorPalette($"wwwroot/{file.Path}", 5))
            {
                var clr = _unitOfWork.ColorRepository.FindById(color.A, color.R, color.G, color.B)
                   ?? _unitOfWork.ColorRepository.Create(new Color()
                   {
                       A = color.A,
                       R = color.R,
                       G = color.G,
                       B = color.B
                   });

                file.Palette.Add(clr);
            }
        }
    }
}
