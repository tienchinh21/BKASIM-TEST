using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Models.Response.Articles;
using MiniAppGIBA.Services.Articles;
using MiniAppGIBA.Services.Articles.ArticleCategories;
using Newtonsoft.Json;

namespace MiniAppGIBA.Controller.CMS
{
    
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ArticleController(IMapper mapper, IArticleService articleService, IArticleCategoryService articleCategoryService, IUnitOfWork unitOfWork) : BaseCMSController
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<IActionResult> Index()
        {
            ViewBag.ArticleCategories = await articleCategoryService.GetAllAsync();
            return View();
        }

        [HttpGet("Article/Create")]
        public async Task<IActionResult> Create()
        {
            var article = new ArticleResponse()
            {
                Id = string.Empty,
                BannerImage = string.Empty,
                Title = string.Empty,
                Content = string.Empty,
                Status = 0,
                LastModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
            };
            ViewBag.Button = "Lưu";
            ViewBag.Title = "Thêm mới tin tức";
            ViewBag.ArticleCategories = await articleCategoryService.GetAllAsync();
            return PartialView("_Article", article);
        }

        [HttpGet("Article/Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            ViewBag.Button = "Cập nhật";
            ViewBag.Title = "Cập nhật tin tức";
            var result = await articleService.GetByIdAsync(id);
            Console.WriteLine("result: " + JsonConvert.SerializeObject(result));
            if (result == null)
            {
                return RedirectToAction("Create");
            }
            ViewBag.ArticleCategories = await articleCategoryService.GetAllAsync();
            return PartialView("_Article", ConvertArticle(result));
        }

        private ArticleResponse ConvertArticle(Article item, bool summarize = true)
        {
            ArticleResponse article = mapper.Map<ArticleResponse>(item);

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

            // Xử lý Images
            article.Images = !string.IsNullOrEmpty(item.Images)
                ? item.Images.Split(',').Select(x => $"{baseUrl}/uploads/images/articles/{x}").ToList()
                : new List<string>();

            // Xử lý BannerImage
            article.BannerImage = !string.IsNullOrEmpty(item.BannerImage)
                ? $"{baseUrl}/uploads/images/articles/{item.BannerImage.Split(',').FirstOrDefault()}"
                : $"{baseUrl}/images/no-image-2.jpg";

            article.SummarizeContent = Tools.SummarizeHtmlContent(article.Content ?? "", 200);
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
                    var names = _unitOfWork.GetRepository<Group>()
                        .AsQueryable()
                        .Where(g => ids.Contains(g.Id))
                        .Select(g => g.GroupName)
                        .ToList();
                    article.GroupNames = names;
                }
            }
            return article;
        }
    }
}
