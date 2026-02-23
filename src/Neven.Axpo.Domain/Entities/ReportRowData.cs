using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class ReportRowData
{
    public string LocalTime { get; set; }
    public double Volume { get; set; }

}