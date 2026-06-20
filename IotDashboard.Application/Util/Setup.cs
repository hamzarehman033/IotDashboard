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
            services.AddScoped<IValidator<SiteVM>, SiteVMValidator>();
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
            services.AddScoped<ISiteHandler, SiteHandler>();
            services.AddScoped<ILookupHandler, LookupHandler>();
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

        public static async Task<IApplicationBuilder> ApplyPendingMigrations(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDBContext>();
                await dbContext.Database.MigrateAsync();

                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await SeedDefaultRoles(roleManager);
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
    }
}
