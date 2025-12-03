using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;

namespace MiniAppGIBA.Models.Queries.Appointment
{
    public class AppointmentQueryParams : RequestQuery, IRequestQuery
    {
        public string? Keyword { get; set; }
        public byte? Status { get; set; }
    }
}