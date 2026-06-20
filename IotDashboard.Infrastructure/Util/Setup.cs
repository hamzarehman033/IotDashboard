using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using IotDashboard.Infrastructure.Persistence;
using IotDashboard.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Infrastructure.Util
{
    public static class Setup
    {
        public static void SetupInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly("IotDashboard.Infrastructure"))
            );
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            RepoInejctor(services);
        }

        private static void RepoInejctor(IServiceCollection services)
        {
            services.AddScoped<IWeatherRepository, WeatherRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
        }

    }
}
