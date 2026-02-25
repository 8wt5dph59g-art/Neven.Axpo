namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// This class is used as base class for all reports that contain tabular data.
/// </summary>
public abstract class ReportTabularData
{
    /// <summary>
    /// Data structure that holds report data. All data is of string type and ready to export to
    /// textual file formats.
    /// </summary>
    public string[,] TabularData { get; init; } = { };
    
    /// <summary>
    /// Data headers.
    /// </summary>
    public string[] Headers { get; set; } = [];
}