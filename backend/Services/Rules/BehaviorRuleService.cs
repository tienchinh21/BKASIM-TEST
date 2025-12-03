using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Common;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helper;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Rules;
using MiniAppGIBA.Models.Request.Rules;
using System.Linq;

namespace MiniAppGIBA.Services.Rules
{
    public class BehaviorRuleService(IUnitOfWork unitOfWork, IUrl urlHelper, ILogger<BehaviorRuleService> logger)
        : Service<BehaviorRule>(unitOfWork), IBehaviorRuleService
    {
        public async Task<BehaviorRule> CreateAsync(CreateBehaviorRuleRequest request, IWebHostEnvironment env, HttpContext httpContext)
        {
            // Normalize values
            var contentType = (request.ContentType ?? string.Empty).Trim().ToUpperInvariant();
            var type = (request.Type ?? string.Empty).Trim().ToUpperInvariant();

            if (contentType != "TEXT" && contentType != "FILE")
            {
                throw new ArgumentException("ContentType phải là TEXT hoặc FILE");
            }

            if (type != "APP" && type != "GROUP")
            {
                throw new ArgumentException("Type phải là APP hoặc GROUP");
            }

            string? groupId = request.GroupId;
            if (type == "GROUP")
            {
                if (string.IsNullOrWhiteSpace(groupId))
                {
                    throw new ArgumentException("GroupId là bắt buộc khi Type = GROUP");
                }

                // Validate group exists
                var groupRepo = unitOfWork.GetRepository<Group>();
                var exists = await groupRepo.AsQueryable().AnyAsync(g => g.Id == groupId);
                if (!exists)
                {
                    throw new ArgumentException("GroupId không tồn tại");
                }
            }

            string content = string.Empty;

            if (contentType == "TEXT")
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    throw new ArgumentException("Content không được trống khi ContentType = TEXT");
                }
                content = request.Content!.Trim();
            }
            else
            {
                if (request.File == null || request.File.Length == 0)
                {
                    throw new ArgumentException("File bắt buộc khi ContentType = FILE");
                }

                var rootPath = env.WebRootPath ?? env.ContentRootPath;
                var baseFolder = Path.Combine(rootPath, "uploads", "behavior_rules");
                string relativeDir;

                if (type == "GROUP" && !string.IsNullOrEmpty(groupId))
                {
                    relativeDir = $"/uploads/behavior_rules/groups/{groupId}";
                }
                else
                {
                    relativeDir = "/uploads/behavior_rules/app";
                }

                var uploadFolder = Path.Combine(rootPath, relativeDir.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                var newName = await FileHandler.SaveFile(request.File!, uploadFolder);
                content = $"{relativeDir}/{newName}";

                // Ensure URL is normalized (optional, store relative)
                // content = await urlHelper.ToFullUrl(content, httpContext);
            }

            var entity = new BehaviorRule
            {
                ContentType = contentType,
                Type = type,
                GroupId = type == "GROUP" ? groupId : null,
                Content = content,
                Title = request.Title,
                SortOrder = request.SortOrder,
                IsActive = true,
                UpdatedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            await CreateAsync(entity);
            return entity;
        }

        public async Task<(BehaviorRule? Rule, int TotalPages)> GetByPageAsync(string type, string? groupId, int page)
        {
            var normalizedType = (type ?? string.Empty).Trim().ToUpperInvariant();
            if (normalizedType != "APP" && normalizedType != "GROUP")
            {
                throw new ArgumentException("Type phải là APP hoặc GROUP");
            }

            var query = _repository.AsQueryable()
                .Where(r => r.Type == normalizedType);

            if (normalizedType == "GROUP")
            {
                if (string.IsNullOrWhiteSpace(groupId))
                {
                    throw new ArgumentException("GroupId là bắt buộc khi Type = GROUP");
                }
                query = query.Where(r => r.GroupId == groupId);
            }

            // Order by SortOrder asc, then CreatedDate asc
            query = query
                .OrderBy(r => r.SortOrder ?? int.MaxValue)
                .ThenBy(r => r.CreatedDate);

            var total = await query.CountAsync();
            if (total == 0)
            {
                return (null, 0);
            }

            // Clamp page to valid range (1..total)
            var index = Math.Max(1, Math.Min(page, total)) - 1;
            var rule = await query.Skip(index).Take(1).FirstOrDefaultAsync();
            return (rule, total);
        }

        public async Task<BehaviorRule?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return await _repository.AsQueryable()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<BehaviorRule> UpdateAsync(UpdateBehaviorRuleRequest request, IWebHostEnvironment env, HttpContext httpContext)
        {
            var newContentType = (request.ContentType ?? string.Empty).Trim().ToUpperInvariant();
            var newType = (request.Type ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                throw new ArgumentException("Id là bắt buộc");
            }
            if (newContentType != "TEXT" && newContentType != "FILE")
            {
                throw new ArgumentException("ContentType phải là TEXT hoặc FILE");
            }
            if (newType != "APP" && newType != "GROUP")
            {
                throw new ArgumentException("Type phải là APP hoặc GROUP");
            }

            var entity = await _repository.AsQueryable().FirstOrDefaultAsync(r => r.Id == request.Id);
            if (entity == null)
            {
                throw new ArgumentException("Không tìm thấy BehaviorRule theo Id");
            }

            // Xác nhận type khớp với entity hiện tại
            if (!string.Equals(entity.Type, newType, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Type không khớp với bản ghi hiện tại");
            }
            if (entity.Type == "GROUP" && string.IsNullOrWhiteSpace(entity.GroupId))
            {
                throw new ArgumentException("Bản ghi GROUP thiếu GroupId");
            }

            if (newContentType == "TEXT")
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    throw new ArgumentException("Content không được trống khi ContentType = TEXT");
                }
                entity.ContentType = "TEXT";
                entity.Content = request.Content!.Trim();
            }
            else
            {
                if (request.File == null || request.File.Length == 0)
                {
                    throw new ArgumentException("File bắt buộc khi ContentType = FILE");
                }

                var rootPath = env.WebRootPath ?? env.ContentRootPath;
                var relativeDir = entity.Type == "GROUP"
                    ? $"/uploads/behavior_rules/groups/{entity.GroupId}"
                    : "/uploads/behavior_rules/app";

                // Xóa file cũ nếu có
                if (string.Equals(entity.ContentType, "FILE", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(entity.Content))
                {
                    var oldUrl = entity.Content!;
                    if (oldUrl.StartsWith("/")) oldUrl = oldUrl.Substring(1);
                    var oldPath = Path.Combine(rootPath, oldUrl);
                    try
                    {
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    catch { /* bỏ qua lỗi xóa */ }
                }

                var uploadFolder = Path.Combine(rootPath, relativeDir.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                var newName = await FileHandler.SaveFile(request.File!, uploadFolder);
                entity.ContentType = "FILE";
                entity.Content = $"{relativeDir}/{newName}";
            }

            // Cập nhật metadata tùy chọn
            if (request.Title != null) entity.Title = request.Title;
            if (request.SortOrder.HasValue) entity.SortOrder = request.SortOrder;
            entity.UpdatedDate = DateTime.Now;

            _repository.Update(entity);
            await unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string id, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id là bắt buộc");
            }

            var entity = await _repository.AsQueryable().FirstOrDefaultAsync(r => r.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Không tìm thấy BehaviorRule theo Id");
            }

            // Nếu là FILE, xóa file vật lý
            if (string.Equals(entity.ContentType, "FILE", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(entity.Content))
            {
                try
                {
                    var rootPath = env.WebRootPath ?? env.ContentRootPath;
                    var fileUrl = entity.Content!;
                    if (fileUrl.StartsWith("/")) fileUrl = fileUrl.Substring(1);
                    var filePath = Path.Combine(rootPath, fileUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log nhưng không chặn việc xóa DB
                    logger.LogWarning(ex, "Không thể xóa file cũ cho BehaviorRule {Id}", id);
                }
            }

            _repository.Delete(entity);
            await unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}