using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Logs;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Logs;
using MiniAppGIBA.Models.Queries.Logs;
using System.Text.Json;

namespace MiniAppGIBA.Services.Logs
{
    public class ActivityLogService : Service<ActivityLog>, IActivityLogService
    {
        public ActivityLogService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Tạo activity log (automatic logging)
        /// </summary>
        public async Task<ActivityLog> LogActivityAsync(CreateActivityLogDto dto)
        {
            var log = new ActivityLog
            {
                AccountId = dto.AccountId,
                ActionType = dto.ActionType,
                Description = dto.Description,
                TargetEntity = dto.TargetEntity,
                TargetId = dto.TargetId,
                Metadata = dto.Metadata
            };

            await _repository.AddAsync(log);
            await unitOfWork.SaveChangesAsync();

            return log;
        }

        /// <summary>
        /// Lấy logs với phân trang (cho SUPER_ADMIN xem tất cả)
        /// </summary>
        public async Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQueryParameters queryParameters)
        {
            var query = _repository.AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(queryParameters.AccountId))
                query = query.Where(l => l.AccountId == queryParameters.AccountId);

            if (!string.IsNullOrEmpty(queryParameters.ActionType))
                query = query.Where(l => l.ActionType == queryParameters.ActionType);

            if (queryParameters.FromDate.HasValue)
                query = query.Where(l => l.CreatedDate >= queryParameters.FromDate.Value);

            if (queryParameters.ToDate.HasValue)
                query = query.Where(l => l.CreatedDate <= queryParameters.ToDate.Value);

            // Keyword search
            if (!string.IsNullOrEmpty(queryParameters.Keyword))
            {
                query = query.Where(l =>
                    l.ActionType.Contains(queryParameters.Keyword) ||
                    (l.Description != null && l.Description.Contains(queryParameters.Keyword)) ||
                    (l.TargetEntity != null && l.TargetEntity.Contains(queryParameters.Keyword)));
            }

            // Total count
            var totalItems = await query.CountAsync();

            // Sorting
            query = !string.IsNullOrEmpty(queryParameters.Sort) && queryParameters.Sort.ToLower() == "asc"
                ? query.OrderBy(l => l.CreatedDate)
                : query.OrderByDescending(l => l.CreatedDate);

            // Pagination
            var logs = await query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            // Join thủ công bằng FK
            var result = new List<ActivityLogDto>();
            foreach (var log in logs)
            {
                var user = await unitOfWork.Context.Users.FindAsync(log.AccountId);

                result.Add(new ActivityLogDto
                {
                    Id = log.Id,
                    AccountId = log.AccountId,
                    AccountFullName = user?.FullName,
                    AccountEmail = user?.Email,
                    ActionType = log.ActionType,
                    Description = log.Description,
                    TargetEntity = log.TargetEntity,
                    TargetId = log.TargetId,
                    Metadata = log.Metadata,
                    CreatedDate = log.CreatedDate
                });
            }

            return new PagedResult<ActivityLogDto>
            {
                Items = result,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParameters.PageSize),
                Page = queryParameters.Page,
                PageSize = queryParameters.PageSize
            };
        }

        /// <summary>
        /// Lấy logs của ADMIN (chỉ logs của chính mình)
        /// </summary>
        public async Task<PagedResult<ActivityLogDto>> GetLogsByAdminAsync(string adminId, ActivityLogQueryParameters queryParameters)
        {
            queryParameters.AccountId = adminId;
            return await GetActivityLogsAsync(queryParameters);
        }

        /// <summary>
        /// Thống kê logs theo ActionType
        /// </summary>
        public async Task<Dictionary<string, int>> GetStatisticsByActionTypeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var logs = await _repository.AsQueryable().ToListAsync();

            if (fromDate.HasValue)
                logs = logs.Where(l => l.CreatedDate >= fromDate.Value).ToList();

            if (toDate.HasValue)
                logs = logs.Where(l => l.CreatedDate <= toDate.Value).ToList();

            return logs
                .GroupBy(l => l.ActionType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Lấy chi tiết activity log theo ID
        /// </summary>
        public async Task<ActivityLogDto?> GetActivityLogByIdAsync(string id)
        {
            var log = await _repository.FindByIdAsync(id);
            if (log == null) return null;

            // Join thủ công bằng FK
            var user = await unitOfWork.Context.Users.FindAsync(log.AccountId);

            return new ActivityLogDto
            {
                Id = log.Id,
                AccountId = log.AccountId,
                AccountFullName = user?.FullName,
                AccountEmail = user?.Email,
                ActionType = log.ActionType,
                Description = log.Description,
                TargetEntity = log.TargetEntity,
                TargetId = log.TargetId,
                Metadata = log.Metadata,
                CreatedDate = log.CreatedDate
            };
        }

        /// <summary>
        /// Export activity logs to Excel
        /// </summary>
        public async Task<byte[]> ExportToExcelAsync(IEnumerable<ActivityLogDto> logs)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Lịch Sử Hoạt Động");

            // Headers
            worksheet.Cells["A1"].Value = "STT";
            worksheet.Cells["B1"].Value = "Thời gian";
            worksheet.Cells["C1"].Value = "Tài khoản";
            worksheet.Cells["D1"].Value = "Email";
            worksheet.Cells["E1"].Value = "Loại hành động";
            worksheet.Cells["F1"].Value = "Mô tả";
            worksheet.Cells["G1"].Value = "Đối tượng";
            worksheet.Cells["H1"].Value = "ID đối tượng";

            // Header styling
            using (var range = worksheet.Cells["A1:H1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 188, 212));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // Data
            int row = 2;
            int stt = 1;
            foreach (var log in logs)
            {
                worksheet.Cells[$"A{row}"].Value = stt++;
                worksheet.Cells[$"B{row}"].Value = log.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cells[$"C{row}"].Value = log.AccountFullName ?? "N/A";
                worksheet.Cells[$"D{row}"].Value = log.AccountEmail ?? "N/A";
                worksheet.Cells[$"E{row}"].Value = log.ActionType;
                worksheet.Cells[$"F{row}"].Value = log.Description ?? "";
                worksheet.Cells[$"G{row}"].Value = log.TargetEntity ?? "";
                worksheet.Cells[$"H{row}"].Value = log.TargetId ?? "";
                row++;
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
            else
            {
                // Set default widths if no data
                worksheet.Column(1).Width = 10;  // STT
                worksheet.Column(2).Width = 20;  // Thời gian
                worksheet.Column(3).Width = 30;  // Tài khoản
                worksheet.Column(4).Width = 30;  // Email
                worksheet.Column(5).Width = 20;  // Loại hành động
                worksheet.Column(6).Width = 40;  // Mô tả
                worksheet.Column(7).Width = 20;  // Đối tượng
                worksheet.Column(8).Width = 30;  // ID đối tượng
            }

            // Add borders
            if (worksheet.Dimension != null)
            {
                using (var range = worksheet.Cells[worksheet.Dimension.Address])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}

