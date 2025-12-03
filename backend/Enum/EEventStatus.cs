namespace MiniAppGIBA.Enum
{
    /// <summary>
    /// Trạng thái sự kiện
    /// </summary>
    public enum EEventStatus : byte
    {
        /// <summary>
        /// 1 - Sắp diễn ra
        /// </summary>
        Upcoming = 1,

        /// <summary>
        /// 2 - Đang diễn ra
        /// </summary>
        Ongoing = 2,

        /// <summary>
        /// 3 - Đã kết thúc (dựa trên thời gian bắt đầu - kết thúc)
        /// </summary>
        Completed = 3
    }
}

