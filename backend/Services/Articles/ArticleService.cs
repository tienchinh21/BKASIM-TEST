using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Articles;
using MiniAppGIBA.Models.Request.Articles;
using MiniAppGIBA.Constants;

namespace MiniAppGIBA.Services.Articles
{
    public class ArticleService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment env, IHangfireOrderingService hangfireOrderingService) : Service<Article>(unitOfWork), IArticleService
    {
        public async Task<PagedResult<Article>> GetPage(ArticleQueryParams query)
        {
            var articles = _repository.AsQueryable();

            // Apply role-based group filtering
            // GIBA has full access - can see ALL articles (no filtering)
            // Non-admin users only see public articles
            if (!string.IsNullOrEmpty(query.Role))
            {
                if (query.Role == CTRole.GIBA)
                {
                    // GIBA: Can see ALL articles (no filtering)
                }
                else
                {
                    // Non-GIBA roles: can only see public articles
                    articles = articles.Where(a => a.Status == 1);
                }
            }

            // Apply other filters
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var keyword = query.Keyword.ToLower().Trim();
                articles = articles.Where(p => p.Title.ToLower().Contains(keyword));
            }

            if (query.Status.HasValue)
            {
                articles = articles.Where(p => p.Status == query.Status.Value);
            }

            if (!string.IsNullOrEmpty(query.CategoryId))
            {
                articles = articles.Where(p => p.CategoryId == query.CategoryId);
            }

            // Filter by GroupCategory (Type)
            if (!string.IsNullOrEmpty(query.Type))
            {
                if (query.Type == "Group")
                {
                    // When Type is "Group", filter articles where GroupCategory is null
                    articles = articles.Where(p => p.GroupCategory == null);
                }
                else
                {
                    // Filter by specific GroupCategory (GIBA, NBD, Club)
                    articles = articles.Where(p => p.GroupCategory == query.Type);
                }
            }

            if (query.StartDate != null)
            {
                articles = articles.Where(p => p.CreatedDate >= query.StartDate);
            }

            if (query.EndDate != null)
            {
                articles = articles.Where(p => p.CreatedDate <= query.EndDate);
            }

            var totalItems = await articles.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);
            var items = await articles
                                .OrderBy(x => x.OrderPriority)
                                .ThenByDescending(x => x.CreatedDate)
                                .Skip(query.Skip)
                                .Take(query.PageSize)
                                .ToListAsync();
            return new PagedResult<Article>()
            {
                Items = items,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<int> CreateAsync(ArticleRequest model)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                ValidateImages(null, null, model.Images);

                // Process uploads first
                var images = await ProcessUploadImages(model.Images);
                var bannerImage = await ProcessUploadBanner(model.BannerImage);

                // Create article entity manually to ensure all required fields are set
                var article = new Article
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Title = model.Title,
                    Content = model.Content,
                    Author = model.Author ?? string.Empty,
                    Status = model.Status,
                    CategoryId = model.CategoryId,
                    Images = images,
                    CreatedBy = model.CreatedBy ?? string.Empty,
                    BannerImage = ValidateBanner(images, bannerImage), // Ensure BannerImage always has a value
                    OrderPriority = 0,
                    GroupCategory = model.GroupCategory, // Category: "NBD", "Club", or "GIBA"
                    GroupIds = model.GroupIds, // Comma-separated group IDs
                    CreatedDate = DateTime.Now,
                    Role = model.Role
                };

                //await base.CreateAsync(article);
                _repository.Add(article);
                var result = await unitOfWork.CommitAsync();
                if (result != 0)
                {
                    if (model.OrderPriority > 1)
                    {
                        hangfireOrderingService.ScheduleInsertAtPositionJob<Article>(article.Id, model.OrderPriority, nameof(Article.OrderPriority));
                    }
                    else
                    {
                        hangfireOrderingService.ScheduleInsertFirstJob<Article>(article.Id, nameof(Article.OrderPriority));
                    }
                }

                return result;
            } 
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateAsync(string id, ArticleRequest model)
        {
            var article = await GetByIdAsync(id);
            if (article == null)
            {
                throw new NotFoundException(200, "Không tìm thấy tin tức này!");
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                ValidateImages(article.Images, model.RemovedOldImages, model.Images);

                int currentOrder = article.OrderPriority;
                // Preserve CreatedBy from existing article if model.CreatedBy is null/empty
                string? originalCreatedBy = article.CreatedBy;

                mapper.Map(model, article);

                article.CategoryId = model.CategoryId;
                article.GroupCategory = model.GroupCategory; // Update category
                article.GroupIds = model.GroupIds; // Update group IDs
                article.Role = model.Role;
                article.OrderPriority = currentOrder;
                
                // Preserve CreatedBy - don't allow it to be set to null during update
                if (string.IsNullOrEmpty(model.CreatedBy))
                {
                    article.CreatedBy = originalCreatedBy ?? string.Empty;
                }
                else
                {
                    article.CreatedBy = model.CreatedBy;
                }

                if (string.IsNullOrEmpty(model.Author))
                {
                    article.Author = string.Empty;
                }

                if (model.Images != null && model.Images.Any())
                {
                    var newImages = await ProcessUploadImages(model.Images);
                    if (!string.IsNullOrEmpty(newImages))
                    {
                        if (string.IsNullOrEmpty(article.Images))
                        {
                            article.Images = newImages;
                        }
                        else
                        {
                            article.Images = string.Join(',', article.Images, newImages);
                        }
                    }
                }

                if (model.RemovedOldImages.Any())
                {
                    foreach (var imageUrl in model.RemovedOldImages)
                    {
                        if (imageUrl != null)
                        {
                            RemoveOldImage(imageUrl, "uploads/images/articles");

                            var imageList = article.Images?.Split(',')
                                    .Where(img => !string.IsNullOrEmpty(img) && img != imageUrl)
                                    .ToArray();

                            article.Images = imageList != null && imageList.Length > 0 ? string.Join(",", imageList) : null;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.RemovedOldBanner))
                {
                    // Store old banner path before removing
                    var oldBannerPath = model.RemovedOldBanner ?? "";
                    article.BannerImage = "";
                    // Delete old banner after updating article
                    RemoveOldImage(oldBannerPath, "uploads/images/articles");
                }
                else
                {
                    if (model.BannerImage != null)
                    {
                        // Store old banner path before uploading new one
                        var oldBannerPath = article.BannerImage ?? "";
                        // Upload new banner first
                        article.BannerImage = await ProcessUploadBanner(model.BannerImage);
                        // Delete old banner after successful upload
                        if (!string.IsNullOrEmpty(oldBannerPath) && oldBannerPath != article.BannerImage)
                        {
                            RemoveOldImage(oldBannerPath, "uploads/images/articles");
                        }
                    }
                }

                article.BannerImage = ValidateBanner(article.Images ?? "", article.BannerImage);

                //await base.UpdateAsync(article);
                _repository.Update(article);

                var result = await unitOfWork.CommitAsync();

                if (model.OrderPriority != currentOrder && result != 0)
                {
                    hangfireOrderingService.ScheduleReorderJob<Article>(id, model.OrderPriority, nameof(Article.OrderPriority));
                }

                return result;
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        public override async Task<int> DeleteByIdAsync(string id)
        {
            var article = await GetByIdAsync(id);
            if (article == null)
            {
                throw new CustomException(200, "Không tìm thấy tin tức này!");
            }

            // Store image paths before deleting article
            var imagesToDelete = article.Images ?? "";
            var bannerToDelete = article.BannerImage ?? "";
            var orderToDelete = article.OrderPriority;

            // Delete article first
            var result = await base.DeleteByIdAsync(id);

            // Only delete images if article was successfully deleted
            if (result != 0)
            {
                // Delete images after successful deletion
                RemoveOldImage(imagesToDelete, "uploads/images/articles");
                RemoveOldImage(bannerToDelete, "uploads/images/articles");
                
                hangfireOrderingService.ScheduleReorderAfterDeleteJob<Article>(orderToDelete, nameof(Article.OrderPriority));
            }

            return result;
        }

        #region "File Handler"

        private async Task<string> ProcessUploadImages(List<IFormFile>? images)
        {
            var stringFiles = string.Empty;
            var savePath = Path.Combine(env.WebRootPath, "uploads/images/articles");

            if (images != null)
            {
                var fileResult = await FileHandler.SaveFiles(images, savePath);
                stringFiles = string.Join(",", fileResult);
            }

            return stringFiles;
        }

        private async Task<string> ProcessUploadBanner(IFormFile? bannerImage)
        {
            var stringFiles = string.Empty;
            var savePath = Path.Combine(env.WebRootPath, "uploads/images/articles");

            if (bannerImage != null)
            {
                var fileResult = await FileHandler.SaveFile(bannerImage, savePath);
                stringFiles = fileResult;
            }

            return stringFiles;
        }

        private void RemoveOldImage(string listImage, string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(listImage))
                return;

            var images = listImage.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                  .Select(x => Path.Combine(env.WebRootPath, rootFolder, x.Trim()))
                                  .ToList();
            
            if (images.Any())
            {
                FileHandler.RemoveFiles(images);
            }
        }

        private void ValidateImages(string? currentImages, List<string>? removedOldImages, List<IFormFile>? newImages)
        {
            // Tách danh sách ảnh hiện tại
            var existingImages = (currentImages ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // Loại bỏ những ảnh bị remove
            if (removedOldImages != null && removedOldImages.Any())
            {
                existingImages = existingImages
                    .Where(img => !removedOldImages.Contains(img))
                    .ToList();
            }

            // 3Đếm ảnh mới upload
            var newImagesCount = newImages?.Count ?? 0;

            // Tính tổng
            var finalImagesCount = existingImages.Count + newImagesCount;

            // ít nhất 1 ảnh
            if (finalImagesCount == 0)
                throw new CustomException("Bài viết phải có ít nhất một ảnh. Vui lòng thêm ảnh trước khi lưu.");

            // tối đa 5 ảnh
            if (finalImagesCount > 5)
                throw new CustomException("Tối đa 5 ảnh cho một tin tức.");
        }

        private string ValidateBanner(string images, string banner)
        {
            // If banner is empty but we have images, use first image as banner
            if (string.IsNullOrEmpty(banner) && !string.IsNullOrEmpty(images))
            {
                var imageList = images.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Where(img => !string.IsNullOrWhiteSpace(img))
                                              .ToArray();
                if (imageList.Length > 0)
                {
                    return imageList[0].Trim();
                }
            }

            // If banner is still empty, return empty string (will be set to default image in ConvertArticle)
            return string.IsNullOrEmpty(banner) ? string.Empty : banner;
        }

        #endregion

    }
}
