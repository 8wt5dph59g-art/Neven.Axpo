using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class AggregatedPowerTrade
{
    public DateTime TimeStamp { get; init; }
    public AggregatedPowerPeriod[] Aggregations { get; set; } = [];

}