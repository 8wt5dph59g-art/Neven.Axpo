namespace Neven.Axpo.Domain.Entities;

public abstract class ReportTabularData
{
    public string[,] TabularData { get; init; } = { };
}