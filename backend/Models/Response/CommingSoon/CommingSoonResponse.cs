using MiniAppGIBA.Models.DTOs.ComingSoon;
using MiniAppGIBA.Models.DTOs.Events;
namespace MiniAppGIBA.Models.Response.CommingSoon
{
    public class CommingSoonResponse
    {
        // Events sử dụng ComingSoonEventDTO mở rộng từ EventDTO,
        // có thêm 2 field: eventGifts, eventSponsors.
        public List<ComingSoonEventDTO> Events { get; set; } = new List<ComingSoonEventDTO>();
        
        public List<NewsletterDTO> Newsletters { get; set; } = new List<NewsletterDTO>();
        
        public List<MeetingDTO> Meetings { get; set; } = new List<MeetingDTO>();
        public List<ShowcaseDTO> Showcases { get; set; } = new List<ShowcaseDTO>();
    }
}
