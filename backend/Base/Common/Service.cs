using Microsoft.EntityFrameworkCore;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Exceptions;
using MiniAppGIBA.Models.Common;
using System.Linq.Expressions;

namespace MiniAppGIBA.Base.Common
{
    public abstract class Service<T>(IUnitOfWork _unitOfWork) : IService<T> where T : class
    {
        public bool IsAdmin { get; set; }
        protected readonly IUnitOfWork unitOfWork = _unitOfWork;
        protected readonly IRepository<T> _repository = _unitOfWork.GetRepository<T>();

        public virtual Task<int> CreateAsync(T entity)
        {
            _repository.Add(entity);
            return unitOfWork.SaveChangesAsync();
        }

        public virtual Task<int> CreateRangeAsync(IEnumerable<T> entities)
        {
            _repository.AddRange(entities);
            return unitOfWork.SaveChangesAsync();
        }

        public virtual Task<int> DeleteAsync(T entitiy)
        {
            _repository.Delete(entitiy);
            return unitOfWork.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteByIdAsync(string id)
        {
            await _repository.DeleteByIdAsync(id);
            return await unitOfWork.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteRangeAsync(IEnumerable<T> entities)
        {
            _repository.DeleteRange(entities);
            return await unitOfWork.SaveChangesAsync();
        }

        public virtual Task<T?> GetByIdAsync(string id)
        {
            return _repository.FindByIdAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.AsQueryable().ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await _repository.FindByIdsAsync(ids);
        }

        public virtual async Task<PagedResult<T>> GetPage(IRequestQuery query)
        {
            var records = _repository.AsQueryable();
            var properties = typeof(T).GetProperties();
            var parameter = Expression.Parameter(typeof(T), "r");
            Expression combinedExpression = null;

            // Query by keyword
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        var propertyAccess = Expression.Property(parameter, property);
                        var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                        var keywordExpression = Expression.Constant(query.Keyword);

                        if (containsMethod == null) continue;

                        var containsExpression = Expression.Call(propertyAccess, containsMethod, keywordExpression);

                        combinedExpression = combinedExpression == null
                            ? containsExpression
                            : Expression.OrElse(combinedExpression, containsExpression);
                    }
                }
            }

            // Sorting
            /*
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                var sortProperty = properties.FirstOrDefault(p => p.Name.Equals(query.OrderBy, StringComparison.OrdinalIgnoreCase));
                if (sortProperty != null)
                {
                    var propertyAccess = Expression.Property(parameter, sortProperty);
                    var lambda = Expression.Lambda(propertyAccess, parameter);

                    var methodName = query.OrderType.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy";
                    var orderByCall = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(T), sortProperty.PropertyType },
                        records.Expression,
                        Expression.Quote(lambda)
                    );

                    records = records.Provider.CreateQuery<T>(orderByCall);
                }
            }
            */

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                records = records.Where(lambda);
            }

            var totalRecords = records.Count();
            Console.WriteLine($"\n\nTotal Record: {totalRecords} \n\n");
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);
            var items = await records
                              .Skip((query.Page - 1) * query.PageSize)
                              .Take(query.PageSize)
                              .ToListAsync();

            return new PagedResult<T>()
            {
                Items = items,
                TotalPages = totalPages,
            };
        }

        public virtual Task<int> UpdateAsync(T entity)
        {
            _repository.Update(entity);
            return unitOfWork.SaveChangesAsync();
        }

        public virtual Task<int> UpdateAsync(string id, T entity)
        {
            var existingEntity = _repository.FindByIdAsync(id);
            if (existingEntity == null)
            {
                throw new CustomException($"Entity with id {id} not found.");
            }
            _repository.Update(entity);
            return unitOfWork.SaveChangesAsync();
        }

        public virtual Task<int> UpdateRangeAsync(IEnumerable<T> entities)
        {
            _repository.UpdateRange(entities);
            return unitOfWork.SaveChangesAsync();
        }

    }
}
