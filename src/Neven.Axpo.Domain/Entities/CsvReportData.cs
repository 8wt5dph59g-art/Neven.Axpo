namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// This class represents report data for export to CSV file format.
/// </summary>
public class CsvReportData : ReportTabularData
{
    /// <summary>
    /// CSV file name.
    /// </summary>
    public string FileName { get; set; }
}