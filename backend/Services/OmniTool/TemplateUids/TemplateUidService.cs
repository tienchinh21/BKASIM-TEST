using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.ETM;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Services.OmniTool.TemplateUids
{
    public class TemplateUidService(IUnitOfWork unitOfWork) : Service<ZaloTemplateUid>(unitOfWork), ITemplateUidService
    {
        public override async Task<PagedResult<ZaloTemplateUid>> GetPage(IRequestQuery query)
        {
            var invoicteTemplateQuery = _repository.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                string keyword = query.Keyword;
                invoicteTemplateQuery = invoicteTemplateQuery.Where(b => b.Name.Contains(keyword));
            }

            var totalItems = await invoicteTemplateQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);
            var items = await invoicteTemplateQuery.OrderByDescending(x => x.CreatedDate)
                                                   .Skip((query.Page - 1) * query.PageSize)
                                                   .Take(query.PageSize)
                                                   .ToListAsync();
            return new PagedResult<ZaloTemplateUid>()
            {
                Items = items,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public override async Task<int> UpdateAsync(string id, ZaloTemplateUid dto)
        {
            var template = await GetByIdAsync(id);
            if (template == null)
            {
                throw new CustomException(200, "Không tìm thấy mẫu hóa đơn!");
            }
            template.Name = dto.Name;
            template.Message = dto.Message;
            template.ListParams = dto.ListParams;
            return await base.UpdateAsync(template);
        }
    }
}
