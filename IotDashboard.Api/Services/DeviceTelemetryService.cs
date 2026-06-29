using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.Persistence;

namespace IotDashboard.Api.Services
{
    public interface IDeviceTelemetryService
    {
        //Task<Response<bool>> CreateDeviceTelemetryRecordAsync(TelecomTelemetryPacket model);
    }
    public class DeviceTelemetryService : IDeviceTelemetryService
    {
        private readonly AppDBContext _dbContext;

        public DeviceTelemetryService(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        //public async Task<Response<bool>> CreateDeviceTelemetryRecordAsync(TelecomTelemetryPacket model)
        //{

        //    return new Response<bool> { Data = true, Success = true };
        //}
    }
}
