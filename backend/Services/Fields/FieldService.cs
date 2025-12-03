using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Entities.Fields;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using MiniAppGIBA.Models.DTOs.Fields;
using MiniAppGIBA.Models.Queries.Fields;
using MiniAppGIBA.Models.Request.Fields;

namespace MiniAppGIBA.Services.Fields
{
    // TODO: Rewrite toàn bộ service để sử dụng FieldChild mới thay vì ParentId approach
    public class FieldService : IFieldService
    {
        private readonly IRepository<Field> _fieldRepository;
        private readonly IRepository<FieldChild> _fieldChildRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FieldService(
            IRepository<Field> fieldRepository,
            IRepository<FieldChild> fieldChildRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _fieldRepository = fieldRepository;
            _fieldChildRepository = fieldChildRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<FieldDTO>> GetFieldsAsync(FieldQueryParameters queryParameters)
        {
            // TODO: Implement với FieldChild structure
            var fields = await _fieldRepository.AsQueryable()
                // .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrderMiniApp)
                .ThenBy(f => f.FieldName)
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            var fieldDTOs = new List<FieldDTO>();
            foreach (var field in fields)
            {
                var children = await _fieldChildRepository.AsQueryable()
                    .Where(fc => fc.FieldId == field.Id)
                    .Select(fc => new FieldChildDTO
                    {
                        Id = fc.Id,
                        ChildName = fc.ChildName,
                        Description = fc.Description,
                        FieldId = fc.FieldId,
                        CreatedDate = fc.CreatedDate,
                        UpdatedDate = fc.UpdatedDate
                    })
                    .ToListAsync();

                fieldDTOs.Add(new FieldDTO
                {
                    Id = field.Id,
                    FieldName = field.FieldName,
                    Description = field.Description,
                    DisplayOrderMiniApp = field.DisplayOrderMiniApp,
                    IsActive = field.IsActive,
                    CreatedDate = field.CreatedDate,
                    UpdatedDate = field.UpdatedDate,
                    Children = children
                });
            }

            var totalCount = await _fieldRepository.AsQueryable().Where(f => f.IsActive).CountAsync();

            return new PagedResult<FieldDTO>
            {
                Items = fieldDTOs,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParameters.PageSize),
                Page = queryParameters.Page,
                PageSize = queryParameters.PageSize
            };
        }

        public async Task<FieldDTO?> GetFieldByIdAsync(string id)
        {
            var field = await _fieldRepository.FindByIdAsync(id);
            if (field == null) return null;

            var children = await _fieldChildRepository.AsQueryable()
                .Where(fc => fc.FieldId == field.Id)
                .Select(fc => new FieldChildDTO
                {
                    Id = fc.Id,
                    ChildName = fc.ChildName,
                    Description = fc.Description,
                    FieldId = fc.FieldId,
                    CreatedDate = fc.CreatedDate,
                    UpdatedDate = fc.UpdatedDate
                })
                .ToListAsync();

            return new FieldDTO
            {
                Id = field.Id,
                FieldName = field.FieldName,
                Description = field.Description,
                DisplayOrderMiniApp = field.DisplayOrderMiniApp,
                IsActive = field.IsActive,
                CreatedDate = field.CreatedDate,
                UpdatedDate = field.UpdatedDate,
                Children = children
            };
        }

        public async Task<FieldDTO> CreateFieldAsync(CreateFieldRequest request)
        {
            // TODO: Implement create với FieldChild
            var field = new Field
            {
                FieldName = request.FieldName,
                Description = request.Description,
                DisplayOrderMiniApp = request.DisplayOrderMiniApp,
                IsActive = request.IsActive
            };

            await _fieldRepository.AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            // Tạo children nếu có
            var children = new List<FieldChildDTO>();
            foreach (var childRequest in request.Children)
            {
                var child = new FieldChild
                {
                    ChildName = childRequest.ChildName,
                    Description = childRequest.Description,
                    FieldId = field.Id,
                };

                await _fieldChildRepository.AddAsync(child);
                children.Add(new FieldChildDTO
                {
                    Id = child.Id,
                    ChildName = child.ChildName,
                    Description = child.Description,
                    FieldId = child.FieldId,
                    CreatedDate = child.CreatedDate,
                    UpdatedDate = child.UpdatedDate
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return new FieldDTO
            {
                Id = field.Id,
                FieldName = field.FieldName,
                Description = field.Description,
                DisplayOrderMiniApp = field.DisplayOrderMiniApp,
                IsActive = field.IsActive,
                CreatedDate = field.CreatedDate,
                UpdatedDate = field.UpdatedDate,
                Children = children
            };
        }

        public async Task<FieldDTO> UpdateFieldAsync(UpdateFieldRequest request)
        {
            var field = await _fieldRepository.FindByIdAsync(request.Id);
            if (field == null)
            {
                throw new NotFoundException("Lĩnh vực không tồn tại!");
            }

            // Update field thông tin cơ bản
            field.FieldName = request.FieldName;
            field.Description = request.Description;
            field.DisplayOrderMiniApp = request.DisplayOrderMiniApp;
            field.IsActive = request.IsActive;

            var existingChildren = await _fieldChildRepository.AsQueryable()
                .Where(fc => fc.FieldId == field.Id)
                .ToListAsync();

            var childrenIdsInRequest = request.Children
                .Where(c => !string.IsNullOrEmpty(c.Id))
                .Select(c => c.Id)
                .ToList();

            var childrenToDelete = existingChildren
                .Where(ec => !childrenIdsInRequest.Contains(ec.Id))
                .ToList();

            foreach (var childToDelete in childrenToDelete)
            {
                _fieldChildRepository.Delete(childToDelete);
            }
            
            var childrenToUpdate = request.Children
                .Where(rc => !string.IsNullOrEmpty(rc.Id))
                .ToList();

            foreach (var childRequest in childrenToUpdate)
            {
                var existingChild = existingChildren.FirstOrDefault(ec => ec.Id == childRequest.Id);
                if (existingChild != null)
                {
                    existingChild.ChildName = childRequest.ChildName;
                    existingChild.Description = childRequest.Description;
                    _fieldChildRepository.Update(existingChild);
                }
            }

            var childrenToAdd = request.Children
                .Where(rc => string.IsNullOrEmpty(rc.Id))
                .ToList();

            foreach (var childRequest in childrenToAdd)
            {
                var newChild = new FieldChild
                {
                    ChildName = childRequest.ChildName,
                    Description = childRequest.Description,
                    FieldId = field.Id,
                };
                await _fieldChildRepository.AddAsync(newChild);
            }

            _fieldRepository.Update(field);
            await _unitOfWork.SaveChangesAsync();

            return await GetFieldByIdAsync(field.Id) ?? throw new CustomException(404, "Lĩnh vực không tồn tại sau khi cập nhật!");
        }

        public async Task<bool> DeleteFieldAsync(string id)
        {
            var field = await _fieldRepository.FindByIdAsync(id);
            if (field == null) return false;

            var children = await _fieldChildRepository.AsQueryable()
                .Where(fc => fc.FieldId == id)
                .ToListAsync();

            _fieldChildRepository.DeleteRange(children);


            _fieldRepository.Delete(field);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<FieldDTO>> GetActiveFieldsAsync()
        {
            var fields = await _fieldRepository.AsQueryable()
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrderMiniApp)
                .ThenBy(f => f.FieldName)
                .ToListAsync();

            var fieldDTOs = new List<FieldDTO>();
            foreach (var field in fields)
            {
                var children = await _fieldChildRepository.AsQueryable()
                        .Where(fc => fc.FieldId == field.Id)
                    .Select(fc => new FieldChildDTO
                    {
                        Id = fc.Id,
                        ChildName = fc.ChildName,
                        Description = fc.Description,
                        FieldId = fc.FieldId,
                        CreatedDate = fc.CreatedDate,
                        UpdatedDate = fc.UpdatedDate
                    })
                    .ToListAsync();

                fieldDTOs.Add(new FieldDTO
                {
                    Id = field.Id,
                    FieldName = field.FieldName,
                    Description = field.Description,
                    DisplayOrderMiniApp = field.DisplayOrderMiniApp,
                    IsActive = field.IsActive,
                    CreatedDate = field.CreatedDate,
                    UpdatedDate = field.UpdatedDate,
                    Children = children
                });
            }

            return fieldDTOs;
        }

        public async Task<List<FieldDTO>> GetParentFieldsAsync()
        {
            // Trả về tất cả fields vì bây giờ không còn parent-child hierarchy
            return await GetActiveFieldsAsync();
        }
    }
}