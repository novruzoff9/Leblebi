using Leblebi.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace Leblebi.Helper;

public static class ExcelHelper
{
    public static byte[] GenerateWorksheet(ExcelReportDto report, int year, int month)
    {
        int countOfDay = DateTime.DaysInMonth(year, month);
        CultureInfo culture = new CultureInfo("az-AZ");
        DateTimeFormatInfo dtfi = culture.DateTimeFormat;
        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"{report.Title}");
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;

            worksheet.Cells[1, 5, 1, 28].Merge = true;
            worksheet.Cells[1, 5].Value = $"{report.Title} üzrə aylıq Report";
            worksheet.Cells[1, 5].Style.Font.Bold = true;
            worksheet.Cells[1, 5].Style.Font.Size = 16;
            worksheet.Cells[1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[2, 3].Value = $"Tarix: {year} {dtfi.GetMonthName(month)}";
            worksheet.Cells[2, 3].Style.Font.Bold = true;
            worksheet.Cells[2, 3].Style.Font.Size = 14;

            worksheet.Cells[4, 2, 4, 1 + countOfDay].Merge = true;
            worksheet.Cells[4, 2].Value = "Ayın Günləri";
            worksheet.Cells[4, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 1; i <= countOfDay; i++)
            {
                worksheet.Cells[5, i + 1].Value = i;
                worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                worksheet.Column(i + 1).Width = 5.66;
            }
            worksheet.Cells[5, countOfDay + 2].Value = "Cəmi";

            worksheet.Column(1).Width = 16;
            for (int i = 0; i < report.Headers.Count(); i++)
            {
                worksheet.Cells[i + 6, 1].Value = report.Headers[i];
                worksheet.Cells[i + 6, 1].Style.Font.Bold = true;

                decimal total = 0;

                var dailyReports = report.Data.Where(Data => Data.Title == report.Headers[i]).FirstOrDefault();
                if (dailyReports != null)
                {
                    foreach (var item in dailyReports.ValueofDay)
                    {
                        worksheet.Cells[i + 6, item.Key + 1].Value = item.Value;
                        total += Convert.ToDecimal(item.Value);
                    }
                }
                worksheet.Cells[i + 6, countOfDay + 2].Value = total;
                worksheet.Cells[i + 6, countOfDay + 2].Style.Font.Bold = true;
            }
            worksheet.Cells[report.Headers.Count() + 6, 1].Value = "Cəmi";
            for (int i = 2; i <= countOfDay + 2; i++)
            {
                decimal total = 0;
                for (int j = 6; j < report.Headers.Count() + 6; j++)
                {
                    total += Convert.ToDecimal(worksheet.Cells[j, i].Value);
                }
                worksheet.Cells[report.Headers.Count() + 6, i].Value = total;
            }

            using (ExcelRange range = worksheet.Cells[4, 1, report.Headers.Count() + 6, countOfDay + 2])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            return package.GetAsByteArray();
        }
    }
}