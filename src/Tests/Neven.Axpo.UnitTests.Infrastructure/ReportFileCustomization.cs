using System;
using AutoFixture;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.UnitTests.Infrastructure;

public class ReportFileCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<CsvReportData>(x => x
            .With(o => o.FileName, $"ReportFile_{DateTime.Now.Ticks}.csv")
            .With(o => o.Headers, ["header1", "header2"])
            .With(o => o.TabularData, new[,] {{ "13:00", "100.20" }, { "14:00", "80.50" }}
            ));
    }
}