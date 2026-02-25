using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Neven.Axpo.UnitTests.Infrastructure;

public class UseCustomizationAttribute(Type customizationType) : CustomizeAttribute
{
    private readonly ICustomization? _customization = (ICustomization) Activator.CreateInstance(customizationType)!;

    public override ICustomization GetCustomization(ParameterInfo parameter) => _customization!;
}
