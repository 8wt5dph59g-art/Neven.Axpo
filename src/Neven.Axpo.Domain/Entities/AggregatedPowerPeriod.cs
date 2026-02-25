using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class AggregatedPowerPeriod
{
    public int Period { get; init; }
    public double AggregatedVolume { get; init; }
}