using System.Globalization;
using System.Reflection;
using System.Text;

namespace IotDashboard.Api.Services
{
    public interface IReportCsvExportService
    {
        byte[] CreateCsv<T>(IReadOnlyCollection<T> rows);
    }

    public class ReportCsvExportService : IReportCsvExportService
    {
        public byte[] CreateCsv<T>(IReadOnlyCollection<T> rows)
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .OrderBy(p => p.MetadataToken)
                .ToArray();

            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", properties.Select(p => EscapeValue(p.Name))));

            foreach (var row in rows)
            {
                var values = properties
                    .Select(p => EscapeValue(FormatValue(p.GetValue(row))))
                    .ToArray();

                builder.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private static string FormatValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }

        private static string EscapeValue(string value)
        {
            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value}\"";
            }

            return value;
        }
    }
}