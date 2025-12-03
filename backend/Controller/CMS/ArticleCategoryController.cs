using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Base.Dependencies.Extensions;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Services.Articles.ArticleCategories;

namespace MiniAppGIBA.Controller.CMS
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ArticleCategoryController(IArticleCategoryService articleCategoryService) : BaseCMSController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("ArticleCategory/Create")]
        public IActionResult Create()
        {
            var article = new ArticleCategory()
            {
                Id = string.Empty,
                Name = string.Empty,
                DisplayOrder = 1,
            };
            ViewBag.Button = "Lưu";
            ViewBag.Title = "Thêm mới danh mục tin tức";
            return PartialView("_ArticleCategory", article);
        }

        [HttpGet("ArticleCategory/Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            ViewBag.Button = "Cập nhật";
            ViewBag.Title = "Cập nhật danh mục tin tức";
            var result = await articleCategoryService.GetByIdAsync(id);

            if (result == null)
            {
                return RedirectToAction("Create");
            }
            return PartialView("_ArticleCategory", result);
        }
    }
}
