using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Neven.Axpo.UnitTests.Infrastructure;

public class AutoMoqDataAttribute() : AutoDataAttribute(new Fixture()
    .Customize(new AutoMoqCustomization()));