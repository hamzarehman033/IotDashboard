using Microsoft.Extensions.DependencyInjection;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Handlers.Implimentation;
using IotDashboard.Infrastructure.Util;
using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using IotDashboard.Application.Dtos.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using IotDashboard.Infrastructure.ExternalServices.Mqtt;


namespace IotDashboard.Application.Util
{
    public static class Setup
    {
        public static void SetupApplication(this IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            services.SetupInfrastructure(connectionString);
            services.SetupValidators();
            services.SetupHandlers();
            services.SetupIdentity();
            services.SetupConfigs(configuration);
            services.SetupTokenValidation(configuration);
            services.SetupMqttClient();
        }
        private static void SetupValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<WeatherVM>, WeatherValidator>();
            services.AddScoped<IValidator<LoginVM>, LoginValidator>();
            services.AddScoped<IValidator<RegisterVM>, RegisterValidator>();
            services.AddScoped<IValidator<CreateUserVM>, CreateUserValidator>();
            services.AddScoped<IValidator<ChangePasswordVM>, ChangePasswordValidator>();
            services.AddScoped<IValidator<ResetPasswordVM>, ResetPasswordValidator>();
            services.AddScoped<IValidator<CustomerDetailVM>, CustomerDetailVMValidator>();
            services.AddScoped<IValidator<SubscriptionDetailVM>, SubscriptionDetailVMValidator>();
            services.AddScoped<IValidator<LocationVM>, LocationVMValidator>();
            services.AddScoped<IValidator<TenantVM>, TenantVMValidator>();
            services.AddScoped<IValidator<DeviceVM>, DeviceVMValidator>();
            services.AddScoped<IValidator<DeviceInfrastructurePatchVM>, DeviceInfrastructurePatchValidator>();
            services.AddScoped<IValidator<LookupVM>, LookupVMValidator>();
            services.AddTransient(typeof(FilterValidator<>));
        }
        private static void SetupHandlers(this IServiceCollection services)
        {
            services.AddScoped<IWeatherHandler, WeatherHandler>();
            services.AddScoped<IUserHandler, UserHandler>();
            services.AddScoped<ICustomerHandler, CustomerHandler>();
            services.AddScoped<ISubscriptionHandler, SubscriptionHandler>();
            services.AddScoped<ILocationHandler, LocationHandler>();
            services.AddScoped<ITenantHandler, TenantHandler>();
            services.AddScoped<IDeviceHandler, DeviceHandler>();
            services.AddScoped<ILookupHandler, LookupHandler>();
            services.AddScoped<ITelemetryHandler, TelemetryHandler>();
        }

        private static void SetupIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<User>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = typeof(PasswordResetTokenProvider<User>).Name.Split("`")[0];
            })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AppDBContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<PasswordResetTokenProvider<User>>("PasswordResetTokenProvider"); 
        }

        private static void SetupConfigs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWTConfigs>(configuration.GetSection("JWTConfigs"));
        }

        private static void SetupTokenValidation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration.GetValue<string>("JWTConfigs:Issuer"),
                ValidAudience = configuration.GetValue<string>("JWTConfigs:Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWTConfigs:Key"))),
                ClockSkew = TimeSpan.Zero
            };
        });


        }

        private static void SetupMqttClient(this IServiceCollection services)
        {
            services.AddSingleton<IMqttClientService, MqttClientService>();
            services.AddSingleton<IDeviceDataCache, DeviceDataCache>();
        }

        public static async Task<IApplicationBuilder> ApplyPendingMigrations(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDBContext>();
                await dbContext.Database.MigrateAsync();

                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await SeedDefaultRoles(roleManager);

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
                await SeedSysAdminUser(userManager, configuration);
            }

            return app;
        }

        private static async Task SeedDefaultRoles(RoleManager<Role> roleManager)
        {
            var defaultRoles = IotDashboard.Domain.Entities.RoleNames.All;

            foreach (var roleName in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }

        private static async Task SeedSysAdminUser(UserManager<User> userManager, IConfiguration configuration)
        {
            var userName = "SysAdmin";
            var email = "SysAdmin@example.com";
            var password = "SysAdmin";

            var existingSysAdmins = await userManager.GetUsersInRoleAsync(RoleNames.SysAdmin);
            var existingUser = await userManager.FindByNameAsync(userName);
            if (existingSysAdmins.Count > 0 || existingUser != null)
            {
                return;
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                CustomerId = null,
                Modules = new List<long>(),
                AssignedCustomerIds = new List<long>()
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed SysAdmin user '{userName}': {string.Join("; ", createResult.Errors.Select(x => x.Description))}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, RoleNames.SysAdmin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign SysAdmin role to '{userName}': {string.Join("; ", roleResult.Errors.Select(x => x.Description))}");
            }
        }
    }
}
