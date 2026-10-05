using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drawing.Shared.Web
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddDefaultCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .AllowAnyMethod());
            });

            return services;
        }
    }
}
