namespace MiniAppGIBA.Enum
{
    /// <summary>
    /// Trạng thái khách mời
    /// </summary>
    public enum EGuestStatus : byte
    {
        /// 0 - Chờ xác lý
        Pending = 0,

        /// 1 - Đã duyệt
        Approved = 1,

        /// 2 - Từ chối
        Rejected = 2,

        /// 3 - Hủy
        Cancelled = 3,

        /// 4 - pending registration
        PendingRegistration = 4,

        /// 5 - Đã đăng ký
        Registered = 5
    }
}

