﻿using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;
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
        private readonly IModelPredictionService _modelPredictionService;

        public FileService(IUnitOfWork unitOfWork, IModelPredictionService modelPredictionService)
        {
            _unitOfWork = unitOfWork;
            _modelPredictionService = modelPredictionService;
        }

        public PagedList<File> Files(FilesQuery query)
        {
            Expression<Func<File, bool>>[] expressions = new Expression<Func<File, bool>>[0];

            if (query.Tags != null && query.Tags.Length > 0)
            {
                AppendTagFilterExpression(ref expressions, query.Tags);
            }

            if (query.Resolutions != null)
            {
                AppendResolutionFilterExpression(ref expressions, ParsePairs(query.Resolutions));
            }

            if (query.AspectRatios != null)
            {
                AppendAspectRatioFilterExpression(ref expressions, ParsePairs(query.AspectRatios));
            }

            return _unitOfWork.FileRepository.GetPaged(query.Page, query.PageSize, new[] { "Tags" }, f => f.Tags.Count(tag => query.Tags.Contains(tag.Name)), false, expressions);
        }



        public PagedList<File> Favorite(FilesQuery query, string id)
        {
            Expression<Func<File, bool>>[] expressions = new Expression<Func<File, bool>>[0];

            if (!string.IsNullOrEmpty(id))
            {
                AppendUserLikedFilterExpression(ref expressions, id);
            }

            if (query.Tags != null && query.Tags.Length > 0)
            {
                AppendTagFilterExpression(ref expressions, query.Tags);
            }

            if (query.Resolutions != null)
            {
                AppendResolutionFilterExpression(ref expressions, ParsePairs(query.Resolutions));
            }

            if (query.AspectRatios != null)
            {
                AppendAspectRatioFilterExpression(ref expressions, ParsePairs(query.AspectRatios));
            }

            return _unitOfWork.FileRepository.GetPaged(query.Page, query.PageSize, new[] { "Tags" }, f => f.Tags.All(tag => query.Tags.Contains(tag.Name)), false, expressions);
        }

        private static void AppendUserLikedFilterExpression(ref Expression<Func<File, bool>>[] expressions, string userId)
        {
            expressions = expressions.Append(f => f.LikedByUsers.Any(l => l.UserId == userId)).ToArray();
        }

        private static void AppendTagFilterExpression(ref Expression<Func<File, bool>>[] expressions, string[] tags)
        {
            expressions = expressions.Append(f => f.Tags.Any(t => tags.Contains(t.Name))).ToArray();
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
            var file = _unitOfWork.FileRepository.FindFirst(
            file => file.Id == id,
            new[]
            {
                    "User",
                    "Tags",
                    "Colors",
                    "LikedByUsers"
            });

            if (file?.Colors.Count == 0)
            {
                CreatePallet(file);
                _unitOfWork.Save();
            }

            return file;
        }

        public List<File> SimilarFiles(File file)
        {
            var similarFiles = _unitOfWork.FileRepository
                .FindAll(f => f.Tags.Any(tag => file.Tags
                .Contains(tag)))
                .OrderByDescending(f => f.Tags.Count(tag => file.Tags.Contains(tag)))
                .Take(8)
                .ToList();

            similarFiles.RemoveAll(f => f.Id == file.Id);

            return similarFiles;
        }

        public void Upload(IFormFile[] uploads, string userId, string[]? tags)
        {

            foreach (var upload in uploads)
            {
                try
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

                    var predictedTags = _modelPredictionService.PredictTags($"wwwroot/{file.PreviewPath}");
                    foreach (var prediction in predictedTags.Where(prediction => prediction.Confidence > -0.5))
                    {
                        var tag = _unitOfWork.TagRepository.FindFirst(tag => tag.Name == prediction.Label.ToLower())
                            ?? _unitOfWork.TagRepository.Create(new Tag()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = prediction.Label.ToLower()
                            });

                        file.Tags.Add(tag);
                    }
                    _unitOfWork.Save();

                    if (tags != null)
                    {
                        AddTagsToFile(file, tags);
                    }

                    CreatePallet(file);

                    _unitOfWork.FileRepository.Create(file);

                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {

                }
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

        public void AddTagsToFile(File file, string[] tags)
        {
            foreach (var tagName in tags)
            {
                var tag = _unitOfWork.TagRepository.FindFirst(tag => tag.Name == tagName.ToLower())
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

                file.Colors.Add(clr);
            }
        }

        public void AddFileToFavorite(string userId, string fileId)
        {
            var user = _unitOfWork.UserRepository.FindFirst(f => f.Id == userId, new[] { "LikedTags" });
            var file = _unitOfWork.FileRepository.FindFirst(f => f.Id == fileId, new[] { "LikedByUsers", "Tags" });

            if (user != null && file != null)
            {
                if (!file.LikedByUsers.Any(ulf => ulf.UserId == userId))
                {
                    var userLikedFile = new UserLikedFile
                    {
                        UserId = userId,
                        FileId = fileId
                    };

                    user.LikedFiles.Add(userLikedFile);

                    foreach (Tag tag in file.Tags)
                    {
                        var userLikedTag = user.LikedTags.FirstOrDefault(t => t.TagId == tag.Id);

                        if (userLikedTag == null)
                        {
                            user.LikedTags.Add(new UserLikedTag
                            {
                                UserId = user.Id,
                                TagId = tag.Id
                            });
                        }
                        else
                        {
                            userLikedTag.Weight++;
                            _unitOfWork.TagRepository.Update(tag);
                        }
                    }

                    _unitOfWork.UserRepository.Update(user);
                    _unitOfWork.FileRepository.Update(file);

                    _unitOfWork.Save();
                }
            }
        }

        public void RemoveFileFromFavorite(string userId, string fileId)
        {
            var user = _unitOfWork.UserRepository.FindFirst(f => f.Id == userId, new[] { "LikedTags", "LikedFiles" });
            var file = _unitOfWork.FileRepository.FindFirst(f => f.Id == fileId, new[] { "LikedByUsers", "Tags" });

            if (user == null || file == null)
            {
                return;
            }

            var ulf = file.LikedByUsers.Find(ulf => ulf.UserId == userId);

            if (ulf == null)
            {
                return;
            }

            foreach (var tag in ulf.File.Tags)
            {
                user.LikedTags.RemoveAll(t => t.TagId == tag.Id);
            }

            user.LikedFiles.Remove(ulf);
            file.LikedByUsers.Remove(ulf);

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.FileRepository.Update(file);

            _unitOfWork.Save();
        }
    }
}
