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
        private const string PdfContentType = "application/pdf";
        private const string CsvContentType = "text/csv";

        private readonly IStatisticService _statisticService;
        private readonly IReportExcelExportService _excelExportService;
        private readonly IReportPdfExportService _pdfExportService;
        private readonly IReportCsvExportService _csvExportService;

        public ReportDownloadService(
            IStatisticService statisticService,
            IReportExcelExportService excelExportService,
            IReportPdfExportService pdfExportService,
            IReportCsvExportService csvExportService)
        {
            _statisticService = statisticService;
            _excelExportService = excelExportService;
            _pdfExportService = pdfExportService;
            _csvExportService = csvExportService;
        }

        public async Task<ReportDownloadResult> DownloadAsync(ReportDownloadRequest request)
        {
            if (request.Format != ReportFileFormat.Excel && request.Format != ReportFileFormat.Json && request.Format != ReportFileFormat.Pdf && request.Format != ReportFileFormat.Csv)
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

            return request.Format switch
            {
                ReportFileFormat.Excel => BuildExcelResult(result.Records, "BatteryReport", "battery-status-report"),
                ReportFileFormat.Json => BuildJsonResult(result, "battery-status-report"),
                ReportFileFormat.Pdf => BuildPdfResult(result.Records, "Battery Status Report", "battery-status-report"),
                ReportFileFormat.Csv => BuildCsvResult(result.Records, "battery-status-report"),
                _ => throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.")
            };
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

            return request.Format switch
            {
                ReportFileFormat.Excel => BuildExcelResult(result.Records, "SolarReport", "solar-status-report"),
                ReportFileFormat.Json => BuildJsonResult(result, "solar-status-report"),
                ReportFileFormat.Pdf => BuildPdfResult(result.Records, "Solar Status Report", "solar-status-report"),
                ReportFileFormat.Csv => BuildCsvResult(result.Records, "solar-status-report"),
                _ => throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.")
            };
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

            return request.Format switch
            {
                ReportFileFormat.Excel => BuildExcelResult(result.Records, "GridReport", "grid-status-report"),
                ReportFileFormat.Json => BuildJsonResult(result, "grid-status-report"),
                ReportFileFormat.Pdf => BuildPdfResult(result.Records, "Grid Status Report", "grid-status-report"),
                ReportFileFormat.Csv => BuildCsvResult(result.Records, "grid-status-report"),
                _ => throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.")
            };
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

            return request.Format switch
            {
                ReportFileFormat.Excel => BuildExcelResult(result.Records, "AlarmReport", "alarm-status-report"),
                ReportFileFormat.Json => BuildJsonResult(result, "alarm-status-report"),
                ReportFileFormat.Pdf => BuildPdfResult(result.Records, "Alarm Status Report", "alarm-status-report"),
                ReportFileFormat.Csv => BuildCsvResult(result.Records, "alarm-status-report"),
                _ => throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.")
            };
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

            return request.Format switch
            {
                ReportFileFormat.Excel => BuildExcelResult(result.Records, "EnergyReport", "energy-consumption-report"),
                ReportFileFormat.Json => BuildJsonResult(result, "energy-consumption-report"),
                ReportFileFormat.Pdf => BuildPdfResult(result.Records, "Energy Consumption Report", "energy-consumption-report"),
                ReportFileFormat.Csv => BuildCsvResult(result.Records, "energy-consumption-report"),
                _ => throw new NotSupportedException($"Format '{request.Format}' is not implemented yet.")
            };
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

        private ReportDownloadResult BuildPdfResult<T>(IReadOnlyCollection<T> rows, string reportTitle, string filePrefix)
        {
            var bytes = _pdfExportService.CreateDocument(rows, reportTitle);
            return new ReportDownloadResult
            {
                Content = bytes,
                ContentType = PdfContentType,
                FileName = $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf"
            };
        }

        private ReportDownloadResult BuildCsvResult<T>(IReadOnlyCollection<T> rows, string filePrefix)
        {
            var bytes = _csvExportService.CreateCsv(rows);
            return new ReportDownloadResult
            {
                Content = bytes,
                ContentType = CsvContentType,
                FileName = $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
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
