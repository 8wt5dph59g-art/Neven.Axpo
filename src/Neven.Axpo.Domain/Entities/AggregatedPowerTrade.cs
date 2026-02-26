using System;

namespace Neven.Axpo.Domain.Entities;

/// <summary>
/// This class represents collection of aggregated power trades per period.
/// </summary>
public class AggregatedPowerTrade
{
    /// <summary>
    /// Time stamp.
    /// </summary>
    public DateTime TimeStamp { get; init; }
    
    /// <summary>
    /// Array of aggregated power trades per period.
    /// </summary>
    public AggregatedPowerPeriod[] Aggregations { get; set; } = [];

}