using Axpo;
using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class AggregatedPowerTrades
{
    public DateTime TimeStamp { get; set; }
    public PowerTrade[] Aggregations { get; set; }
}