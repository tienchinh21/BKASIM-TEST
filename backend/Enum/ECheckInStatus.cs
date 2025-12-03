namespace MiniAppGIBA.Enum
{
    /// <summary>
    /// Trạng thái check-in sự kiện
    /// </summary>
    public enum ECheckInStatus : byte
    {
        /// <summary>
        /// 1 - Chưa check-in
        /// </summary>
        NotCheckIn = 1,

        /// <summary>
        /// 2 - Đã check-in
        /// </summary>
        CheckedIn = 2,

        /// <summary>
        /// 3 - Đã hủy
        /// </summary>
        Cancelled = 3
    }
}

