using MiniAppGIBA.Base.Database;
using MiniAppGIBA.Base.Interface;
using MiniAppGIBA.Base.Helper;

namespace MiniAppGIBA.Base.Common
{
    public static class Injector
    {
        /// <summary>
        /// Đăng ký các dependency vào IServiceCollection để sử dụng Dependency Injection (DI).
        /// </summary>
        /// <param name="services">Đối tượng IServiceCollection để đăng ký các service.</param>
        public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Tự động đăng ký tất cả các service có tên kết thúc bằng "Service".
            var serviceTypes = typeof(Program).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
                .ToList();

            foreach (var implementationType in serviceTypes)
            {
                // Tìm interface có tên bắt đầu bằng "I" và khớp với tên của implementation class.
                var interfaceType = implementationType.GetInterface($"I{implementationType.Name}");

                // Nếu interface tồn tại, đăng ký nó vào DI container với phạm vi Scoped.
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, implementationType);
                }
            }
            // Đăng ký UnitOfWork theo phạm vi (Scoped).
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Đăng ký Repository theo phạm vi (Scoped).
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Đăng ký service sắp xếp object
            services.AddScoped<IEntityOrderingService, EntityOrderingService>();

            // Đăng ký Hangfire Ordering Service (wrapper)
            services.AddScoped<IHangfireOrderingService, HangfireOrderingService>();

            // Đăng ký Url helper
            services.AddScoped<IUrl, Url>();

            // Đăng ký HomePinRepository
            services.AddScoped<Services.HomePins.IHomePinRepository, Services.HomePins.HomePinRepository>();

            // Đăng ký Custom Field Services
            services.AddScoped<Services.CustomFields.ICustomFieldTabService, Services.CustomFields.CustomFieldTabService>();
            services.AddScoped<Services.CustomFields.ICustomFieldService, Services.CustomFields.CustomFieldService>();
            services.AddScoped<Services.CustomFields.ICustomFieldValueService, Services.CustomFields.CustomFieldValueService>();
            services.AddScoped<Services.CustomFields.ICustomFieldFormHandler, Services.CustomFields.CustomFieldFormHandler>();
            services.AddScoped<Services.CustomFields.ICustomFieldValidator, Services.CustomFields.CustomFieldValidator>();
            services.AddScoped<Services.CustomFields.IGroupCustomFieldService, Services.CustomFields.GroupCustomFieldService>();
        }

    }
}
