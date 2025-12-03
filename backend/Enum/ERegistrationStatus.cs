namespace MiniAppGIBA.Enum
{
    /// <summary>
    /// Trạng thái đăng ký tham gia sự kiện
    /// </summary>
    public enum ERegistrationStatus : short
    {
        /// <summary>
        /// 0 - Chờ duyệt
        /// </summary>
        Pending = 0,
        /// <summary>
        /// 1 - Đã đăng ký
        /// </summary>
        Registered = 1,

        /// <summary>
        /// 2 - Đã check-in
        /// </summary>
        CheckedIn = 2,

        /// <summary>
        /// 3 - Hủy đăng ký
        /// </summary>
        Cancelled = 3
    }
}

