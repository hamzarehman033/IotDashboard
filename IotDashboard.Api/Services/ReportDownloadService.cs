using IotDashboard.Application.Dtos;
using System.Text.Json;

namespace IotDashboard.Api.Services
{
    public class ReportDownloadResult
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "report.bin";
    }

    public interface IReportDownloadService
    {
        Task<ReportDownloadResult> DownloadAsync(ReportDownloadRequest request);
    }

    public class ReportDownloadService : IReportDownloadService
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string JsonContentType = "application/json";

        private readonly IStatisticService _statisticService;
        private readonly IReportExcelExportService _excelExportService;

        public ReportDownloadService(
            IStatisticService statisticService,
            IReportExcelExportService excelExportService)
        {
            _statisticService = statisticService;
            _excelExportService = excelExportService;
        }

        public async Task<ReportDownloadResult> DownloadAsync(ReportDownloadRequest request)
        {
            if (request.Format != ReportFileFormat.Excel && request.Format != ReportFileFormat.Json)
            {
                throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.");
            }

            return request.ReportType switch
            {
                ReportType.BatteryStatus => await BuildBatteryDownloadAsync(request),
                ReportType.SolarStatus => await BuildSolarDownloadAsync(request),
                ReportType.GridStatus => await BuildGridDownloadAsync(request),
                ReportType.AlarmStatus => await BuildAlarmDownloadAsync(request),
                ReportType.EnergyConsumption => await BuildEnergyDownloadAsync(request),
                _ => throw new NotSupportedException($"Report type '{request.ReportType}' is not supported.")
            };
        }

        private async Task<ReportDownloadResult> BuildBatteryDownloadAsync(ReportDownloadRequest request)
        {
            var result = await _statisticService.GetBatteryStatusReport(new BatteryStatusReportRequest
            {
                DeviceId = request.DeviceId,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                TimeRange = request.TimeRange
            });

            return request.Format == ReportFileFormat.Excel
                ? BuildExcelResult(result.Records, "BatteryReport", "battery-status-report")
                : BuildJsonResult(result, "battery-status-report");
        }

        private async Task<ReportDownloadResult> BuildSolarDownloadAsync(ReportDownloadRequest request)
        {
            var result = await _statisticService.GetSolarStatusReport(new SolarStatusReportRequest
            {
                DeviceId = request.DeviceId,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                TimeRange = request.TimeRange
            });

            return request.Format == ReportFileFormat.Excel
                ? BuildExcelResult(result.Records, "SolarReport", "solar-status-report")
                : BuildJsonResult(result, "solar-status-report");
        }

        private async Task<ReportDownloadResult> BuildGridDownloadAsync(ReportDownloadRequest request)
        {
            var result = await _statisticService.GetGridStatusReport(new GridStatusReportRequest
            {
                DeviceId = request.DeviceId,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                TimeRange = request.TimeRange
            });

            return request.Format == ReportFileFormat.Excel
                ? BuildExcelResult(result.Records, "GridReport", "grid-status-report")
                : BuildJsonResult(result, "grid-status-report");
        }

        private async Task<ReportDownloadResult> BuildAlarmDownloadAsync(ReportDownloadRequest request)
        {
            var result = await _statisticService.GetAlarmStatusReport(new AlarmStatusReportRequest
            {
                DeviceId = request.DeviceId,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                TimeRange = request.TimeRange
            });

            return request.Format == ReportFileFormat.Excel
                ? BuildExcelResult(result.Records, "AlarmReport", "alarm-status-report")
                : BuildJsonResult(result, "alarm-status-report");
        }

        private async Task<ReportDownloadResult> BuildEnergyDownloadAsync(ReportDownloadRequest request)
        {
            var result = await _statisticService.GetEnergyConsumptionReport(new EnergyConsumptionReportRequest
            {
                DeviceId = request.DeviceId,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                TimeRange = request.TimeRange
            });

            return request.Format == ReportFileFormat.Excel
                ? BuildExcelResult(result.Records, "EnergyReport", "energy-consumption-report")
                : BuildJsonResult(result, "energy-consumption-report");
        }

        private ReportDownloadResult BuildExcelResult<T>(IReadOnlyCollection<T> rows, string worksheetName, string filePrefix)
        {
            var bytes = _excelExportService.CreateWorkbook(rows, worksheetName);
            return new ReportDownloadResult
            {
                Content = bytes,
                ContentType = ExcelContentType,
                FileName = $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx"
            };
        }

        private static ReportDownloadResult BuildJsonResult<T>(T payload, string filePrefix)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return new ReportDownloadResult
            {
                Content = bytes,
                ContentType = JsonContentType,
                FileName = $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json"
            };
        }
    }
}
