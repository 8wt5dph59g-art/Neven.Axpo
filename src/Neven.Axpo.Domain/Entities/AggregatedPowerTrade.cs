using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class AggregatedPowerTrade
{
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public AggregatedPowerPeriod[] Aggregations { get; set; } = [];
}