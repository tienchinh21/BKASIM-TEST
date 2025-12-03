using MiniAppGIBA.Models.DTOs.ComingSoon;
using MiniAppGIBA.Models.DTOs.Events;
namespace MiniAppGIBA.Models.Response.CommingSoon
{
    public class CommingSoonResponse
    {
	        // Events s dng ComingSoonEventDTO 10e3c mdf rd9ng teb EventDTO,
	        // ca tha tham 2 field: eventGifts, eventSponsors.
	        public List<ComingSoonEventDTO> Events { get; set; } = new List<ComingSoonEventDTO>();
        public NewsletterDTO Newsletter { get; set; } = new NewsletterDTO();
        public List<MeetingDTO> Meetings { get; set; } = new List<MeetingDTO>();
        public List<ShowcaseDTO> Showcases { get; set; } = new List<ShowcaseDTO>();
    }

}