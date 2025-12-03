using System.ComponentModel.DataAnnotations;

namespace MiniAppGIBA.Models.Request.Events
{
    public class ApproveGuestListRequest
    {
        [Required]
        public bool IsApproved { get; set; }

        public string? RejectReason { get; set; }
    }
}

