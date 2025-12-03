using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Request.Articles;

namespace MiniAppGIBA.Services.Articles.ArticleCategories
{
    public class ArticleCategoryService(IUnitOfWork unitOfWork, IHangfireOrderingService orderingService) : Service<ArticleCategory>(unitOfWork), IArticleCategoryService
    {
        public override async Task<PagedResult<ArticleCategory>> GetPage(IRequestQuery query)
        {
            var categories = _repository.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                categories = categories.Where(x => x.Name.Contains(query.Keyword));
            }

            var totalItems = await categories.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);
            var skip = (query.Page - 1) * query.PageSize;
            var listCategories = await categories.OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreatedDate).Skip(skip).Take(query.PageSize).ToListAsync();

            return new PagedResult<ArticleCategory>()
            {
                Items = listCategories,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<int> CreateAsync(ArticleCategoryRequest request)
        {
            await EnsureUniqueNameAsync(request.Name);
            var category = new ArticleCategory() { Name = request.Name, DisplayOrder = request.DisplayOrder };
            var result = await base.CreateAsync(category);
            if (result != 0)
            {
                if (request.DisplayOrder > 1)
                {
                    orderingService.ScheduleInsertAtPositionJob<ArticleCategory>(category.Id, request.DisplayOrder, nameof(ArticleCategory.DisplayOrder));
                }
                else
                {
                    orderingService.ScheduleInsertFirstJob<ArticleCategory>(category.Id, nameof(ArticleCategory.DisplayOrder));
                }
            }
            return result;
        }

        public async Task<int> UpdateAsync(string id, ArticleCategoryRequest request)
        {
            var category = await GetByIdAsync(id);
            if (category == null) throw new CustomException(1, "Không tìm thấy danh mục.");
            await EnsureUniqueNameAsync(request.Name, excludeId: id);

            int currentOrder = category.DisplayOrder;
            category.Name = request.Name;
            category.DisplayOrder = request.DisplayOrder;

            var result = await base.UpdateAsync(category);

            if (request.DisplayOrder != currentOrder && result != 0)
            {
                orderingService.ScheduleReorderJob<ArticleCategory>(id, request.DisplayOrder, nameof(ArticleCategory.DisplayOrder));
            }

            return result;
        }

        public async Task<int> DeleteAsync(string id)
        {
            var existingCategory = await GetByIdAsync(id);
            if (existingCategory == null) throw new CustomException(1, "Không tìm thấy danh mục.");
            var orderToDelete = existingCategory.DisplayOrder;
            var result = await base.DeleteByIdAsync(id);

            if (result != 0)
            {
                orderingService.ScheduleReorderAfterDeleteJob<ArticleCategory>(orderToDelete, nameof(ArticleCategory.DisplayOrder));
            }

            return result;
        }

        public async Task<int> DeleteRange(List<string> categoryIds)
        {
            var categories = await _repository.AsQueryable().Where(x => categoryIds.Contains(x.Id)).ToListAsync();
            if (!categories.Any()) throw new CustomException(1, "Không tìm thấy danh mục nào!");

            _repository.DeleteRange(categories);

            var result = await unitOfWork.SaveChangesAsync();

            if (result != 0)
            {
                orderingService.ScheduleValidateAndFixJob<ArticleCategory>(nameof(ArticleCategory.DisplayOrder));
            }

            return result;
        }

        public override async Task<IEnumerable<ArticleCategory>> GetAllAsync()
        {
            return await _repository.AsQueryable().OrderBy(x => x.DisplayOrder).ToListAsync();
        }

        #region Validation

        private async Task EnsureUniqueNameAsync(string name, string? excludeId = null)
        {
            if (string.IsNullOrEmpty(name)) throw new CustomException(1, "Vui lòng nhập tên danh mục!");
            var query = _repository.AsQueryable().Where(c => c.Name == name);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(c => c.Id != excludeId);
            }

            bool exists = await query.AnyAsync();
            if (exists)
            {
                throw new CustomException(1, $"Tên danh mục \"{name}\" đã tồn tại.");
            }
        }

        #endregion
    }
}
