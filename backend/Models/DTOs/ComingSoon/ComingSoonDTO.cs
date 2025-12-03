namespace MiniAppGIBA.Models.DTOs.ComingSoon
{
    // public class EventDTO
    // {
    //     public string Id { get; set; } = string.Empty;
    //     public string Title { get; set; } = string.Empty;
    //     public DateTime StartTime { get; set; }
    //     public DateTime EndTime { get; set; }
    //     public string Location { get; set; } = string.Empty;
    // }

    public class NewsletterDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BannerImage { get; set; } = string.Empty;
        public int Type { get; set; } // 0 = Nội bộ, 1 = Công khai
        public string TypeText { get; set; } = string.Empty; // "Nội bộ" hoặc "Công khai"
    }
    public class MeetingDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Chủ đề
        public DateTime Time { get; set; }
        public int MeetingType { get; set; } // 1 = Online, 2 = Offline
        public string? Location { get; set; } // Cho offline
        public string? MeetingLink { get; set; } // Cho online
        public string GroupName { get; set; } = string.Empty; // Hội nhóm
        public bool IsPinned { get; set; } = false;
        public int Type { get; set; } // 0 = Nội bộ, 1 = Công khai
        public string TypeText { get; set; } = string.Empty; // "Nội bộ" hoặc "Công khai"
    }

    public class ShowcaseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MembershipName { get; set; } = string.Empty; // Diễn giả
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public string GroupName { get; set; } = string.Empty; // Hội nhóm
        public bool IsPinned { get; set; } = false;
        public int Type { get; set; } // 0 = Nội bộ, 1 = Công khai
        public string TypeText { get; set; } = string.Empty; // "Nội bộ" hoặc "Công khai"
    }
}

