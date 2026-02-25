using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class CsvReportData : ReportTabularData
{
    public string FileName { get; set; }
    public string[] Headers { get; set; } = [];
}