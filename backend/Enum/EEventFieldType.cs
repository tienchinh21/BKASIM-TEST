namespace MiniAppGIBA.Enum
{
    /// Kiểu dữ liệu cho trường tùy chỉnh của sự kiện
    public enum EEventFieldType : byte
    {
        /// 1 - Văn bản (Text)
        Text = 1,

        /// 2 - Số nguyên (Integer)
        Integer = 2,

        /// 3 - Số thập phân (Decimal)
        Decimal = 3,

        /// 4 - Năm sinh (Year of birth)
        YearOfBirth = 4,

        /// 5 - Boolean (Đúng/Sai)
        Boolean = 5,

        /// 6 - Ngày giờ (Date and time)    
        DateTime = 6,

        /// 7 - Ngày (Date)
        Date = 7,

        /// 8 - Email
        Email = 8,

        /// 9 - Số điện thoại (Phone number)
        PhoneNumber = 9,

        /// 10 - Đường dẫn URL (URL link)
        Url = 10,

        /// 11 - Văn bản dài (Long text)
        LongText = 11,

        /// 12 - Danh sách lựa chọn (Dropdown list)
        Dropdown = 12,

        /// 13 - Đa lựa chọn (Multiple choice)
        MultipleChoice = 13,

        /// 14 - File đính kèm (Attached file)
        File = 14,

        /// 15 - Hình ảnh (Image)
        Image = 15
    }
}

