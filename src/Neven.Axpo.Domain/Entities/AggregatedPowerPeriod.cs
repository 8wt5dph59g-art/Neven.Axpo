namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// This class holds information about aggregated volume in trade for a certain period.
/// </summary>
public class AggregatedPowerPeriod
{
    /// <summary>
    /// Trade period with example value like '02.02.2026 15:00'
    /// </summary>
    public DateTime Period { get; init; }
    
    /// <summary>
    /// Aggregated volume of trades that can be null if there was no volumes for given period.
    /// </summary>
    public double? AggregatedVolume { get; init; }
}