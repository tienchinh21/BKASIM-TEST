using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Models.Queries.Articles;
using MiniAppGIBA.Models.Request.Articles;
using MiniAppGIBA.Models.Response.Articles;
using MiniAppGIBA.Services.Articles;
using MiniAppGIBA.Services.Articles.ArticleCategories;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
using Microsoft.EntityFrameworkCore;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ArticlesController(ILogger<ArticlesController> logger, IMapper mapper, IArticleService articleService, IArticleCategoryService articleCategoryService, IUnitOfWork unitOfWork) : ControllerBase
    {
        private ArticleResponse ConvertArticle(Article item, bool summarize = true)
        {
            ArticleResponse article = mapper.Map<ArticleResponse>(item);

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

            // Xử lý Images
            article.Images = !string.IsNullOrEmpty(item.Images)
                ? item.Images.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => $"{baseUrl}/uploads/images/articles/{x}").ToList()
                : new List<string>();

            // Xử lý BannerImage
            if (!string.IsNullOrEmpty(item.BannerImage))
            {
                var bannerParts = item.BannerImage.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (bannerParts.Length > 0)
                {
                    article.BannerImage = $"{baseUrl}/uploads/images/articles/{bannerParts[0].Trim()}";
                }
            }

            // If no banner, use first image
            if (string.IsNullOrEmpty(article.BannerImage) && article.Images != null && article.Images.Any())
            {
                article.BannerImage = article.Images.FirstOrDefault();
            }

            // If still no banner, use default image
            if (string.IsNullOrEmpty(article.BannerImage))
            {
                article.BannerImage = $"{baseUrl}/images/no-image-1.jpg";
            }

            article.SummarizeContent = Tools.SummarizeHtmlContent(article.Content ?? string.Empty, 200);
            article.GroupCategory = item.GroupCategory; // Map GroupCategory
            article.GroupIds = item.GroupIds; // Map GroupIds

            // Resolve group names when scope is defined by explicit group IDs (GroupCategory = null)
            if (string.IsNullOrEmpty(item.GroupCategory) && !string.IsNullOrEmpty(item.GroupIds))
            {
                var ids = item.GroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .ToList();
                if (ids.Any())
                {
                    var names = unitOfWork.GetRepository<Group>()
                        .AsQueryable()
                        .Where(g => ids.Contains(g.Id))
                        .Select(g => g.GroupName)
                        .ToList();
                    article.GroupNames = names;
                }
            }
            return article;
        }

       
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicArticles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? categoryId = null,
            [FromQuery] string? groupType = null,
            [FromQuery] string? type = null)
        {
            try
            {
                var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                var userZaloId = User?.FindFirst("UserZaloId")?.Value;

                var articles = unitOfWork.GetRepository<Article>().AsQueryable();

                if (!isAuthenticated || string.IsNullOrEmpty(userZaloId))
                {
                    articles = articles.Where(a => a.Status == 1);
                }
                else
                {
                    var membership = await unitOfWork.GetRepository<Membership>()
                        .AsQueryable()
                        .Where(m => m.UserZaloId == userZaloId && m.IsDelete != true)
                        .FirstOrDefaultAsync();

                    if (membership == null)
                    {
                        articles = articles.Where(a => a.Status == 1);
                    }
                    else
                    {
                        var userGroupMemberships = await unitOfWork.GetRepository<MembershipGroup>()
                            .AsQueryable()
                            .Where(mg => mg.UserZaloId == userZaloId && mg.IsApproved == true)
                            .Include(mg => mg.Group)
                            .ToListAsync();

                        var userGroupIds = userGroupMemberships.Select(mg => mg.GroupId).ToList();

                        // Simplified: show public articles or articles where user is in the group
                        articles = articles.Where(a =>
                            a.Status == 1 || 
                            (a.Status == 0 && (
                                a.GroupCategory == CTRole.GIBA ||
                                (!string.IsNullOrEmpty(a.GroupIds) && userGroupIds.Any(gid => a.GroupIds.Contains(gid)))
                            ))
                        );
                    }
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    var lowerKeyword = keyword.ToLower().Trim();
                    articles = articles.Where(a => a.Title.ToLower().Contains(lowerKeyword) ||
                                                   (a.Content != null && a.Content.ToLower().Contains(lowerKeyword)));
                }

                if (!string.IsNullOrEmpty(categoryId))
                {
                    articles = articles.Where(a => a.CategoryId == categoryId);
                }

                // Filter by GroupCategory - support both 'groupType' and 'type' parameters for consistency
                var filterType = !string.IsNullOrEmpty(type) ? type : groupType;
                if (!string.IsNullOrEmpty(filterType))
                {
                    if (filterType == "Group")
                    {
                        // When Type is "Group", filter articles where GroupCategory is null
                        articles = articles.Where(a => a.GroupCategory == null);
                    }
                    else
                    {
                        // Filter by specific GroupCategory (GIBA, NBD, Club)
                        articles = articles.Where(a => a.GroupCategory == filterType);
                    }
                }

                // Get total count
                var totalItems = await articles.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Get paginated results
                var items = await articles
                    .OrderBy(a => a.OrderPriority)
                    .ThenByDescending(a => a.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to response DTOs
                var data = items.Select(article => ConvertArticle(article, true));

                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Data = data,
                    totalPages = totalPages,
                    totalCount = totalItems,
                    currentPage = pageNumber,
                    pageSize = pageSize,
                    isAuthenticated = isAuthenticated
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving public articles: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = "Internal Server Error",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("GetPage")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPage([FromQuery] ArticleQueryParams query)
        {
            try
            {
                // Only GIBA admin has full access, others see only published articles
                if (!User.IsInRole(CTRole.GIBA))
                {
                    query.Status = 1;
                }

                var role = User.GetRoles().FirstOrDefault();
                query.Role = role;
                query.CreatedBy = User.GetUserId();

                // GIBA has full access to all articles - no filtering needed
                if (role == CTRole.GIBA)
                {
                    query.UserGroupIds = null;
                }

                var result = await articleService.GetPage(query);
                var data = result.Items.Select(article => ConvertArticle(article, true));
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Thành công",
                        Data = data,
                        totalPages = result.TotalPages,
                        totalCount = result.TotalItems,
                        currentPage = result.Page,
                        pageSize = result.PageSize
                    });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                // Log the full exception with stack trace
                logger.LogError(ex, "Error occurred while retrieving articles page: {Message}\n{StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new
                {
                    Code = 1,
                    Message = $"Internal Server Error: {ex.Message}",
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("Category")]
        [AllowAnonymous]
        public async Task<ActionResult> Category()
        {
            try
            {
                var data = await articleCategoryService.GetAllAsync();
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công",
                    Data = data
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(string id)
        {
            try
            {
                var article = await articleService.GetByIdAsync(id);
                if (article == null)
                {
                    return StatusCode(404, "Không tìm thấy tin tức!");
                }
                return Ok(new
                {
                    Code = 0,
                    Message = "Thành công.",
                    Data = ConvertArticle(article, false)
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<ActionResult> CreateArticle([FromForm] ArticleRequest model)
        {
            try
            {
                var role = User.GetRoles().FirstOrDefault();

                model.CreatedBy = User.GetUserId() ?? string.Empty;
                model.Role = role;
                
                // Simplified: No GroupCategory/GroupIds processing needed
                // All articles are accessible to GIBA admin

                var result = await articleService.CreateAsync(model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Tạo tin tức thành công!",
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                Console.WriteLine("ex: " + ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<ActionResult> UpdateArticle(string id, [FromForm] ArticleRequest model)
        {
            try
            {
                var role = User.GetRoles().FirstOrDefault();
                model.Role = role;

                // Simplified: No GroupCategory/GroupIds processing needed
                // All articles are accessible to GIBA admin

                var article = await articleService.UpdateAsync(id, model);
                return Ok(new
                {
                    Code = 0,
                    Message = "Cập nhật tin tức thành công!",
                    Data = article
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPatch("{id}/Status")]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<ActionResult> UpdateStatus(string id, short status)
        {
            try
            {
                //var article = await _articleService.UpdateStatus(id, status);
                await Task.CompletedTask;
                return Ok(new
                {
                    Code = 0,
                    Message = $"Cập nhật trạng thái {(status == (short)MiniAppGIBA.Enum.EArticle.Public ? "công khai" : "riêng tư")} thành công!",
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = CTRole.GIBA)]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await articleService.DeleteByIdAsync(id);
                return Ok(new
                {
                    Code = 0,
                    Message = "Xóa tin tức thành công!",
                });
            }
            catch (CustomException ex)
            {
                return StatusCode(ex.Code, new
                {
                    Code = 1,
                    ex.Message,
                    ex.Data
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

    }
}
