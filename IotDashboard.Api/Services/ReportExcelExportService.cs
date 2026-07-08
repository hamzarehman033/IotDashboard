using System.Reflection;
using ClosedXML.Excel;

namespace IotDashboard.Api.Services
{
    public interface IReportExcelExportService
    {
        byte[] CreateWorkbook<T>(IReadOnlyCollection<T> rows, string worksheetName);
    }

    public class ReportExcelExportService : IReportExcelExportService
    {
        public byte[] CreateWorkbook<T>(IReadOnlyCollection<T> rows, string worksheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(worksheetName) ? "Report" : worksheetName);

            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .OrderBy(p => p.MetadataToken)
                .ToArray();

            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cell(1, col + 1).Value = properties[col].Name;
            }

            var headerRange = worksheet.Range(1, 1, 1, Math.Max(properties.Length, 1));
            headerRange.Style.Font.Bold = true;

            int rowIndex = 2;
            foreach (var row in rows)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(row);
                    var cell = worksheet.Cell(rowIndex, col + 1);
                    SetCellValue(cell, value);
                }

                rowIndex++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void SetCellValue(IXLCell cell, object? value)
        {
            if (value is null)
            {
                cell.Value = string.Empty;
                return;
            }

            switch (value)
            {
                case DateTime dateTime:
                    cell.Value = dateTime;
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                    break;
                case DateTimeOffset dateTimeOffset:
                    cell.Value = dateTimeOffset.UtcDateTime;
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                    break;
                case Enum:
                    cell.Value = value.ToString();
                    break;
                default:
                    cell.Value = value.ToString() ?? string.Empty;
                    break;
            }
        }
    }
}
