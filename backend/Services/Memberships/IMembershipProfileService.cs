namespace MiniAppGIBA.Services.Memberships
{
    public interface IMembershipProfileService
    {
        /// <summary>
        /// Lấy thông tin profile công khai của thành viên theo slug
        /// </summary>
        Task<Dictionary<string, object>?> GetProfileBySlugAsync(string slug, string? groupId = null);
    }
}

