using IotDashboard.Api.Util;
using IotDashboard.Api.Hubs;
using IotDashboard.Api.Services;
using IotDashboard.Application.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IotDashboard API", Version = "v1" });
    c.OperationFilter<FiltersSwaggerConfigs>();
    // Add JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // Must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

builder.Services.SetupApplication(builder.Configuration.GetConnectionString("default"), builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IMqttPayloadDecoder, MqttPayloadDecoder>();
builder.Services.AddSingleton<ITelemetryPersistenceService, TelemetryPersistenceService>();
builder.Services.AddTransient<IStatisticService, StatisticService>();
builder.Services.AddScoped<IDeviceDataService, DeviceDataService>();
builder.Services.AddHostedService<MqttConnectionHostedService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());


await app.ApplyPendingMigrations();
app.UseMiddleware<LocalizationMiddleware>();
app.MapControllers();
app.MapHub<DeviceDataHub>("/hubs/device-data");

// Initialize device data service for MQTT to SignalR integration
using (var scope = app.Services.CreateScope())
{
    var deviceDataService = scope.ServiceProvider.GetRequiredService<IDeviceDataService>();
    await deviceDataService.InitializeAsync();
}

app.Run();
