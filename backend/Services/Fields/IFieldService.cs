using MiniAppGIBA.Entities.Fields;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Fields;
using MiniAppGIBA.Models.Queries.Fields;
using MiniAppGIBA.Models.Request.Fields;

namespace MiniAppGIBA.Services.Fields
{
    public interface IFieldService
    {
        Task<PagedResult<FieldDTO>> GetFieldsAsync(FieldQueryParameters queryParameters);
        Task<FieldDTO?> GetFieldByIdAsync(string id);
        Task<FieldDTO> CreateFieldAsync(CreateFieldRequest request);
        Task<FieldDTO> UpdateFieldAsync(UpdateFieldRequest request);
        Task<bool> DeleteFieldAsync(string id);
        Task<List<FieldDTO>> GetActiveFieldsAsync();

        /// <summary>
        /// Lấy danh sách lĩnh vực cha (ParentId = null)
        /// </summary>
        Task<List<FieldDTO>> GetParentFieldsAsync();
    }
}
