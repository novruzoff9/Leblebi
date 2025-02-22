namespace Leblebi.DTOs;

public class ExcelReportDto
{
    public string Title { get; set; }
    public List<string> Headers { get; set; }
    public List<DailyReportDto> Data { get; set; }
}

public class DailyReportDto
{
    public string Title { get; set; }
    public Dictionary<int, string> ValueofDay { get; set; }
}
