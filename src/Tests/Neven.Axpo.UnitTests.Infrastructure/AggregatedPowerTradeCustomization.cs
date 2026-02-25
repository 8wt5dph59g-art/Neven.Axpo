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
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 3, 0, 0), AggregatedVolume = 200.06622d},
                new AggregatedPowerPeriod {Period = new DateTime(2026, 2, 2, 18, 0, 0), AggregatedVolume = -75.06622d}])
            );
    }
}