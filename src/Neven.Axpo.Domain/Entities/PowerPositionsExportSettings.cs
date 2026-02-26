namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// Class that contains Intra-Day export configuration data.
/// </summary>
public class PowerPositionsExportSettings
{
    /// <summary>
    /// Interval in minutes to trigger report export
    /// </summary>
    public int IntervalInMinutes { get; set; }
    
    /// <summary>
    /// Path where to store report exports.
    /// </summary>
    public string ReportPath { get; set; }
}