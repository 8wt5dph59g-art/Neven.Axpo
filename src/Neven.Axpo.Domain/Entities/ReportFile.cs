using JetBrains.Annotations;

namespace Neven.Axpo.Domain.Entities;

[UsedImplicitly]
public class ReportFile
{
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public string[] Headers { get; set; }
    public string[][] Rows { get; set; }
}