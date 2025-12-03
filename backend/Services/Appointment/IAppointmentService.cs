using MiniAppGIBA.Entities.Appointment;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Appointment;
using MiniAppGIBA.Models.Request.Appointments;
using MiniAppGIBA.Models.DTOs.Appointments;

namespace MiniAppGIBA.Services.Appointment
{
    public interface IAppointmentService : IService<Entities.Appointment.Appointment>
    {
        Task<PagedResult<AppointmentDetailDTO>> GetPage(AppointmentQueryParams query);
        Task<int> Create(AppointmentRequest model);
        Task<PagedResult<AppointmentDetailDTO>> GetAppointmentFilter(string userId,string? type, int? status);
        Task<bool> Update(string appointmentId, string userId, UpdateAppointmentRequest request);
        Task<bool> UpdateStatus(string appointmentId, UpdateAppointmentStatusRequest request);
        Task<AppointmentDetailDTO?> GetAppointmentDetailById(string id);
    }
}