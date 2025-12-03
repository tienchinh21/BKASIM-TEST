using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Groups;
using MiniAppGIBA.Models.Response.Common;

namespace MiniAppGIBA.Models.Response.Groups
{
    public class GroupResponse : ApiResponse<GroupDTO>
    {
    }

    public class GroupDetailResponse : ApiResponse<GroupDetailDTO>
    {
    }

    public class GroupListResponse : ApiResponse<PagedResult<GroupDTO>>
    {
    }
}
