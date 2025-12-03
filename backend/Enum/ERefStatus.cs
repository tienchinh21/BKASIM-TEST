namespace MiniAppGIBA.Enum
{
    /// <summary>
    /// Trạng thái Ref
    /// </summary>
    public enum ERefStatus : byte
    {
        /// <summary>
        /// 1 - Đã gửi
        /// </summary>
        Sent = 1,

        /// <summary>
        /// 2 - Đã nhận
        /// </summary>
        Received = 2,

        /// <summary>
        /// 3 - Hoàn thành
        /// </summary>
        Completed = 3
    }
}

