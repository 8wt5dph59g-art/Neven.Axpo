using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class AggregatedPowerPeriod
{
    public int Period { get; set; }
    public double AggregatedVolume { get; set; } = 0.0;
}