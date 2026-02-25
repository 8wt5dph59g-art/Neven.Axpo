namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// Helper class containing header names and delimiter.
/// </summary>
public static class IntraDayCsvReportConfiguration
{
    /// <summary>
    /// Value for Local Time header.
    /// </summary>
    public const string HeaderLocalTime = "Local Time";
    
    /// <summary>
    /// Value for Volume header.
    /// </summary>
    public const string HeaderVolume = "Volume";
    
    /// <summary>
    /// Delimiter used for CSV Export.
    /// </summary>
    public const string CsvDelimiter = ";";
}