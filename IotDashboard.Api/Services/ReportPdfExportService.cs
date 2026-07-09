using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IotDashboard.Api.Services
{
    public interface IReportPdfExportService
    {
        byte[] CreateDocument<T>(IReadOnlyCollection<T> rows, string title);
    }

    public class ReportPdfExportService : IReportPdfExportService
    {
        public byte[] CreateDocument<T>(IReadOnlyCollection<T> rows, string title)
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .OrderBy(p => p.MetadataToken)
                .ToArray();

            var safeTitle = string.IsNullOrWhiteSpace(title) ? "Report" : title;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text(safeTitle)
                        .SemiBold()
                        .FontSize(16);

                    page.Content().Element(content =>
                    {
                        if (properties.Length == 0)
                        {
                            content.Text("No data columns available.");
                            return;
                        }

                        content.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (var i = 0; i < properties.Length; i++)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var property in properties)
                                {
                                    header.Cell().Element(HeaderCellStyle).Text(property.Name);
                                }
                            });

                            if (rows.Count == 0)
                            {
                                table.Cell()
                                    .ColumnSpan((uint)properties.Length)
                                    .Element(BodyCellStyle)
                                    .Text("No records found.");
                            }
                            else
                            {
                                foreach (var row in rows)
                                {
                                    foreach (var property in properties)
                                    {
                                        var value = property.GetValue(row);
                                        table.Cell().Element(BodyCellStyle).Text(FormatValue(value));
                                    }
                                }
                            }
                        });
                    });

                    page.Footer()
                        .AlignRight()
                        .Text($"Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                });
            }).GeneratePdf();
        }

        private static string FormatValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                _ => value.ToString() ?? string.Empty
            };
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .DefaultTextStyle(x => x.SemiBold());
        }

        private static IContainer BodyCellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4)
                .PaddingHorizontal(4);
        }
    }
}