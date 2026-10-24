
using System;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nalogovaya
{
    public class PdfReportService
    {
        public static void CreateTasksReport(DataTable tasks, string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(style => style.FontSize(9));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("ОТЧЁТ ПО ЗАДАЧАМ")
                                .FontSize(20)
                                .Bold();

                            column.Item()
                                .Text("Дата формирования: " +
                                      DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

                            column.Item().PaddingBottom(10);
                        });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("№")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Проект")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Задача")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Сотрудник")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Срок")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Статус")
                                .FontColor(Colors.White)
                                .Bold();

                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text("Приоритет")
                                .FontColor(Colors.White)
                                .Bold();
                        });

                        foreach (DataRow row in tasks.Rows)
                        {
                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["TaskId"].ToString());

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["ProjectName"].ToString());

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["Title"].ToString());

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["AssignedToName"] == DBNull.Value
                                    ? "Не назначен"
                                    : row["AssignedToName"].ToString());

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["DueDate"] == DBNull.Value
                                    ? "—"
                                    : Convert.ToDateTime(row["DueDate"])
                                        .ToString("dd.MM.yyyy"));

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["Status"].ToString());

                            table.Cell()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .Text(row["Priority"].ToString());
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Система управления проектами • Страница ");
                            text.CurrentPageNumber();
                        });
                });
            }).GeneratePdf(filePath);
        }
    }
}
