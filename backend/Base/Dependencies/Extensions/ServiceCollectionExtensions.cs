namespace MiniAppGIBA.Base.Dependencies.Extensions
{
    public static class ServiceCollectionExtensions
    {        /// <summary>
             /// Add all application services at once
             /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDatabaseServices(configuration);
            //services.AddHangfireServices(configuration);
            //services.AddAuthenticationServices(configuration);
            services.AddMediatRServices();
            //services.AddCorsServices();
            //services.AddSwaggerServices();
            //services.AddIdentityServices();
            //services.AddBusinessServices();
            //services.AddRepositoryServices();
            //services.AddValidationServices();
            //services.AddAutoMapperServices();

            return services;
        }
    }
}
