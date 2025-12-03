using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Request.Articles;
using MiniAppGIBA.Services.Articles.ArticleCategories;

namespace MiniAppGIBA.Controller.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
    public class ArticleCategoriesController(ILogger<ArticleCategoriesController> logger, IArticleCategoryService articleCategoryService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetPage([FromQuery] RequestQuery query)
        {
            try
            {
                var result = await articleCategoryService.GetPage(query);
                return Ok(new
                {
                    Code = 200,
                    Message = "Thành công",
                    Data = result.Items,
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
                logger.LogDebug("Error occurred while retrieving all products: {0}", ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ArticleCategoryRequest model)
        {
            try
            {
                var result = await articleCategoryService.CreateAsync(model);
                return Ok(new
                {
                    Code = 200,
                    Message = "Tạo danh mục tin tức thành công!",
                    Data = result
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

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] ArticleCategoryRequest model)
        {
            try
            {
                var article = await articleCategoryService.UpdateAsync(id, model);
                return Ok(new
                {
                    Code = 200,
                    Message = "Cập nhật danh mục tin tức thành công!",
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await articleCategoryService.DeleteAsync(id);
                return Ok(new
                {
                    Code = 0,
                    Message = "Xóa danh mục tin tức thành công!",
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

        [HttpPost("QuickDelete")]
        public async Task<IActionResult> QuickDelete([FromBody] List<string> categoryIds)
        {
            try
            {
                if (categoryIds == null || categoryIds.Count == 0)
                {
                    return Ok(new
                    {
                        Code = 1,
                        Message = "Vui lòng chọn ít nhất một danh mục!",
                    });
                }

                var rs = await articleCategoryService.DeleteRange(categoryIds);

                if (rs > 0)
                {
                    return Ok(new
                    {
                        Code = 0,
                        Message = "Xóa các danh mục thành công!",
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Code = 1,
                        Message = "Xóa các danh mục thất bại!",
                    });
                }

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
                logger.LogError(ex.Message);
                return StatusCode(200, new
                {
                    Code = 1,
                    Message = "Internal Server Error"
                });
            }
        }
    }
}
