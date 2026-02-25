using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class ReportFile
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string[] Headers { get; set; } = [];
    public string[,] TabularData { get; init; } = { };
}