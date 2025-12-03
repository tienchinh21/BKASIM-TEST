// using MiniAppGIBA.Models.DTOs.Memberships;

// namespace MiniAppGIBA.Services.Memberships
// {
//     /// <summary>
//     /// Interface cho MembershipFKService - sử dụng Foreign Keys thay vì Navigation Properties
//     /// </summary>
//     public interface IMembershipFKService
//     {
//         /// <summary>
//         /// Lấy danh sách members với thông tin groups (sử dụng FK)
//         /// </summary>
//         Task<List<MembershipWithGroupsDTO>> GetMembersWithGroupsAsync();

//         /// <summary>
//         /// Lấy thống kê membership theo groups (sử dụng FK)
//         /// </summary>
//         Task<Dictionary<string, int>> GetMembershipStatsByGroupAsync();

//         /// <summary>
//         /// Lấy members đã tham gia events (sử dụng FK)
//         /// </summary>
//         Task<List<MemberEventParticipationDTO>> GetMemberEventParticipationAsync();

//         /// <summary>
//         /// Lấy top members có nhiều groups nhất (sử dụng FK)
//         /// </summary>
//         Task<List<TopMemberDTO>> GetTopMembersByGroupCountAsync(int limit = 10);
//     }
// }
