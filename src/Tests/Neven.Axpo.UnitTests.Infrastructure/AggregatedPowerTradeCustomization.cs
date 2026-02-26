using System;
using AutoFixture;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.UnitTests.Infrastructure;

public class AggregatedPowerTradeCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<AggregatedPowerTrade>(x => x
            .With(o => o.TimeStamp, new DateTime(2026, 2, 1, 15, 0, 0))
            .With(o => o.Aggregations, [
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 1, 23, 0, 0), AggregatedVolume = 100d}, 
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 0, 0, 0), AggregatedVolume = 200d},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 1, 0, 0), AggregatedVolume = 200.06622d},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 2, 0, 0), AggregatedVolume = -75.06622d},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 3, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 4, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 5, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 6, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 7, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 8, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 9, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 10, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 11, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 12, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 13, 0, 0), AggregatedVolume = null},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 14, 0, 0), AggregatedVolume = null}])
            );
    }
}