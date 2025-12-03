using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.Queries.Appointment;
using MiniAppGIBA.Base.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using MiniAppGIBA.Base.Common;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Models.Request.Appointments;
using MiniAppGIBA.Enum;
using MiniAppGIBA.Constants;
using MiniAppGIBA.Models.DTOs.Appointments;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Memberships;
namespace MiniAppGIBA.Services.Appointment;

public class AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment env, IHangfireOrderingService hangfireOrderingService) : Service<Entities.Appointment.Appointment>(unitOfWork), IAppointmentService
{
    public async Task<PagedResult<AppointmentDetailDTO>> GetPage(AppointmentQueryParams query)
    {
        var queryable = _repository.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            queryable = queryable.Where(a =>
                (a.Name != null && a.Name.ToLower().Contains(keyword)) ||
                (a.Content != null && a.Content.ToLower().Contains(keyword)) ||
                (a.Location != null && a.Location.ToLower().Contains(keyword))
            );
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(a => a.Status == query.Status.Value);
        }

        // Get total count before pagination
        var totalItems = await queryable.CountAsync();

        // Apply pagination
        var appointments = await queryable
            .OrderByDescending(a => a.CreatedDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // Get all unique UserZaloIds and GroupIds
        var fromUserIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.AppointmentFrom))
            .Select(a => a.AppointmentFrom!)
            .Distinct()
            .ToList();
        
        var toUserIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.AppointmentTo))
            .Select(a => a.AppointmentTo!)
            .Distinct()
            .ToList();
        
        var groupIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.GroupId))
            .Select(a => a.GroupId!)
            .Distinct()
            .ToList();

        // Load all Memberships and Groups in one query
        var membershipRepo = unitOfWork.GetRepository<Membership>();
        var groupRepo = unitOfWork.GetRepository<Group>();
        
        var allUserIds = fromUserIds.Union(toUserIds).ToList();
        var memberships = await membershipRepo.AsQueryable()
            .Where(m => allUserIds.Contains(m.UserZaloId!))
            .ToListAsync();
        
        var groups = await groupRepo.AsQueryable()
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync();

        // Create lookup dictionaries for fast access
        var membershipLookup = memberships.ToDictionary(m => m.UserZaloId!, m => m);
        var groupLookup = groups.ToDictionary(g => g.Id, g => g);

        // Map appointments to DTOs
        var appointmentDTOs = appointments.Select(appointment =>
        {
            var fromMember = !string.IsNullOrEmpty(appointment.AppointmentFrom) 
                && membershipLookup.TryGetValue(appointment.AppointmentFrom, out var from) 
                ? from : null;
            
            var toMember = !string.IsNullOrEmpty(appointment.AppointmentTo) 
                && membershipLookup.TryGetValue(appointment.AppointmentTo, out var to) 
                ? to : null;
            
            var group = !string.IsNullOrEmpty(appointment.GroupId) 
                && groupLookup.TryGetValue(appointment.GroupId, out var g) 
                ? g : null;

            return new AppointmentDetailDTO
            {
                Id = appointment.Id,
                Name = appointment.Name,
                AppointmentFromId = appointment.AppointmentFrom,
                AppointmentToId = appointment.AppointmentTo,
                GroupId = appointment.GroupId,
                AppointmentFromName = fromMember?.Fullname,
                AppointmentToName = toMember?.Fullname,
                AppointmentFromAvatar = fromMember?.ZaloAvatar,
                AppointmentToAvatar = toMember?.ZaloAvatar,
                GroupName = group?.GroupName,
                CancelReason = appointment.CancelReason,
                Content = appointment.Content,
                Location = appointment.Location,
                Time = appointment.Time,
                Status = appointment.Status,
                StatusText = GetStatusText(appointment.Status),
                CreatedDate = appointment.CreatedDate,
                UpdatedDate = appointment.UpdatedDate
            };
        }).ToList();

        return new PagedResult<AppointmentDetailDTO>
        {
            Items = appointmentDTOs,
            TotalPages = (int)Math.Ceiling((double)totalItems / query.PageSize),
            TotalItems = totalItems,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }
    public async Task<int> Create(AppointmentRequest model)
    {
        var appointment = new Entities.Appointment.Appointment();
        appointment.Name = model.Name;
        appointment.AppointmentFrom = model.AppointmentFrom;
        appointment.GroupId = model.GroupId;
        appointment.AppointmentTo = model.AppointmentTo;
        appointment.Content = model.Content;
        appointment.Location = model.Location;
        appointment.Time = model.Time;
        appointment.Status = (byte)EAppointmentStatus.Sent;
        appointment.CreatedDate = DateTime.Now;
        appointment.UpdatedDate = DateTime.Now;
        var result = await base.CreateAsync(appointment);
      
        return result;
    }
    public async Task<PagedResult<AppointmentDetailDTO>> GetAppointmentFilter(string userId,string? type, int? status)
    {
        var query = _repository.AsQueryable();
        if(type == CTTypeAppointment.From)
        {
            query = query.Where(a => a.AppointmentFrom == userId);
        }
        else if(type == CTTypeAppointment.To)
        {
            query = query.Where(a => a.AppointmentTo == userId);
        }
        else{
            query = query.Where(a => a.AppointmentFrom == userId || a.AppointmentTo == userId);
        }
        if(status != null)
        {
            query = query.Where(a => a.Status == status);
        }
        var appointments = await query
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        // Get all unique UserZaloIds and GroupIds
        var fromUserIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.AppointmentFrom))
            .Select(a => a.AppointmentFrom!)
            .Distinct()
            .ToList();
        
        var toUserIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.AppointmentTo))
            .Select(a => a.AppointmentTo!)
            .Distinct()
            .ToList();
        
        var groupIds = appointments
            .Where(a => !string.IsNullOrEmpty(a.GroupId))
            .Select(a => a.GroupId!)
            .Distinct()
            .ToList();

        // Load all Memberships and Groups in one query
        var membershipRepo = unitOfWork.GetRepository<Membership>();
        var groupRepo = unitOfWork.GetRepository<Group>();
        
        var allUserIds = fromUserIds.Union(toUserIds).ToList();
        var memberships = await membershipRepo.AsQueryable()
            .Where(m => allUserIds.Contains(m.UserZaloId!))
            .ToListAsync();
        
        var groups = await groupRepo.AsQueryable()
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync();

        // Create lookup dictionaries for fast access
        var membershipLookup = memberships.ToDictionary(m => m.UserZaloId!, m => m);
        var groupLookup = groups.ToDictionary(g => g.Id, g => g);

        // Map appointments to DTOs
        var appointmentDTOs = appointments.Select(appointment =>
        {
            var fromMember = !string.IsNullOrEmpty(appointment.AppointmentFrom) 
                && membershipLookup.TryGetValue(appointment.AppointmentFrom, out var from) 
                ? from : null;
            
            var toMember = !string.IsNullOrEmpty(appointment.AppointmentTo) 
                && membershipLookup.TryGetValue(appointment.AppointmentTo, out var to) 
                ? to : null;
            
            var group = !string.IsNullOrEmpty(appointment.GroupId) 
                && groupLookup.TryGetValue(appointment.GroupId, out var g) 
                ? g : null;

            return new AppointmentDetailDTO
            {
                Id = appointment.Id,
                Name = appointment.Name,
                AppointmentFromId = appointment.AppointmentFrom,
                AppointmentToId = appointment.AppointmentTo,
                GroupId = appointment.GroupId,
                AppointmentFromName = fromMember?.Fullname,
                AppointmentToName = toMember?.Fullname,
                AppointmentFromAvatar = fromMember?.ZaloAvatar,
                AppointmentToAvatar = toMember?.ZaloAvatar,
                GroupName = group?.GroupName,
                CancelReason = appointment.CancelReason,
                Content = appointment.Content,
                Location = appointment.Location,
                Time = appointment.Time,
                Status = appointment.Status,
                StatusText = GetStatusText(appointment.Status),
                CreatedDate = appointment.CreatedDate,
                UpdatedDate = appointment.UpdatedDate
            };
        }).ToList();

        return new PagedResult<AppointmentDetailDTO>
        {
            Items = appointmentDTOs,
            TotalItems = appointmentDTOs.Count,
            Page = 1,
            PageSize = appointmentDTOs.Count,
            TotalPages = 1
        };
    }

    public async Task<bool> Update(string appointmentId, string userId, UpdateAppointmentRequest request)
    {
        var appointment = await GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return false;
        }

        // Kiểm tra user là AppointmentFrom và status là 1 (Sent)
        if (appointment.AppointmentFrom != userId || appointment.Status != (byte)EAppointmentStatus.Sent)
        {
            return false;
        }

        // Cập nhật các trường nếu có giá trị
        if (!string.IsNullOrEmpty(request.Name))
        {
            appointment.Name = request.Name;
        }
        if (!string.IsNullOrEmpty(request.GroupId))
        {
            appointment.GroupId = request.GroupId;
        }
        if (!string.IsNullOrEmpty(request.AppointmentTo))
        {
            appointment.AppointmentTo = request.AppointmentTo;
        }
        if (request.Content != null)
        {
            appointment.Content = request.Content;
        }
        if (request.Location != null)
        {
            appointment.Location = request.Location;
        }
        if (request.Time.HasValue)
        {
            appointment.Time = request.Time.Value;
        }

        appointment.UpdatedDate = DateTime.Now;
        _repository.Update(appointment);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateStatus(string appointmentId, UpdateAppointmentStatusRequest request)
    {
        var appointment = await GetByIdAsync(appointmentId);
        if (appointment == null || appointment.Status == (byte)EAppointmentStatus.Confirmed)
        {
            return false;
        }

        // Nếu status == 3 (Cancelled) thì phải có CancelReason
        if (request.Status == (byte)EAppointmentStatus.Cancelled && string.IsNullOrEmpty(request.CancelReason))
        {
            return false;
        }

        appointment.Status = request.Status;
        if (request.Status == (byte)EAppointmentStatus.Cancelled)
        {
            appointment.CancelReason = request.CancelReason;
        }
        else
        {
            // Nếu không phải status 3 thì clear CancelReason
            appointment.CancelReason = null;
        }

        appointment.UpdatedDate = DateTime.Now;
        _repository.Update(appointment);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<AppointmentDetailDTO?> GetAppointmentDetailById(string id)
    {
        var appointment = await GetByIdAsync(id);
        if (appointment == null)
        {
            return null;
        }

        // Get Group name
        string? groupName = null;
        if (!string.IsNullOrEmpty(appointment.GroupId))
        {
            var group = await unitOfWork.GetRepository<Group>()
                .GetFirstOrDefaultAsync(g => g.Id == appointment.GroupId);
            groupName = group?.GroupName;
        }

        // Get AppointmentFrom name and avatar (Membership)
        string? fromName = null;
        string? fromAvatar = null;
        if (!string.IsNullOrEmpty(appointment.AppointmentFrom))
        {
            var fromMember = await unitOfWork.GetRepository<Membership>()
                .GetFirstOrDefaultAsync(m => m.UserZaloId == appointment.AppointmentFrom && m.IsDelete != true);
            fromName = fromMember?.Fullname;
            fromAvatar = fromMember?.ZaloAvatar;
        }

        // Get AppointmentTo name and avatar (Membership)
        string? toName = null;
        string? toAvatar = null;
        if (!string.IsNullOrEmpty(appointment.AppointmentTo))
        {
            var toMember = await unitOfWork.GetRepository<Membership>()
                .GetFirstOrDefaultAsync(m => m.UserZaloId == appointment.AppointmentTo && m.IsDelete != true);
            toName = toMember?.Fullname;
            toAvatar = toMember?.ZaloAvatar;
        }

        return new AppointmentDetailDTO
        {
            Id = appointment.Id,
            Name = appointment.Name,
            AppointmentFromId = appointment.AppointmentFrom,
            AppointmentToId = appointment.AppointmentTo,
            GroupId = appointment.GroupId,
            AppointmentFromName = fromName,
            AppointmentToName = toName,
            AppointmentFromAvatar = fromAvatar,
            AppointmentToAvatar = toAvatar,
            GroupName = groupName,
            CancelReason = appointment.CancelReason,
            Content = appointment.Content,
            Location = appointment.Location,
            Time = appointment.Time,
            Status = appointment.Status,
            StatusText = GetStatusText(appointment.Status),
            CreatedDate = appointment.CreatedDate,
            UpdatedDate = appointment.UpdatedDate
        };
    }

    private string GetStatusText(byte status)
    {
        return status switch
        {
            1 => "Đã gửi",
            2 => "Đã xác nhận",
            3 => "Đã hủy",
            _ => "Không xác định"
        };
    }
}