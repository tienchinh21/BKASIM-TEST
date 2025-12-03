using System.Reflection;

namespace MiniAppGIBA.Base.Dependencies.Extensions
{
    public static class MediatRServiceExtensions
    {
        /// <summary>
        /// Configure MediatR services
        /// </summary>
        public static IServiceCollection AddMediatRServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
