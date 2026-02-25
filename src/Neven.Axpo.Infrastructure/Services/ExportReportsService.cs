using FluentResults;
using JetBrains.Annotations;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;
using Serilog;

namespace Neven.Axpo.Infrastructure.Services;

[UsedImplicitly]
public class ExportReportsService(ILogger logger) : IExportReportsService
{
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));
    
    public async Task<Result> ExportToCsvFileAsync(CsvReportFileData csvReportFileData, string exportFolder, bool includeHeaders = true)
    {
        if (string.IsNullOrWhiteSpace(csvReportFileData.FileName))
        {
            return Result.Fail("Report file name must be defined.");
        }

        if (string.IsNullOrWhiteSpace(exportFolder))
        {
            return Result.Fail("Report file path must be defined.");
        }

        var numberOfHeaderColumns = csvReportFileData.Headers.Length;
        if (includeHeaders && numberOfHeaderColumns == 0)
        {
            return Result.Fail("Data headers are missing.");
        }

        var columnsNumberInTabularData = csvReportFileData.TabularData.GetLength(1);
        if (includeHeaders && numberOfHeaderColumns != columnsNumberInTabularData)
        {
            return Result.Fail("Number of header columns does not match number of columns in tabular data.");
        }

        var lines = new List<string>();
        if (includeHeaders)
        {
            lines.Add(CreateFileLine(csvReportFileData.Headers));
        }

        var rowsNumberInTabularData = csvReportFileData.TabularData.GetLength(0);
        for (var i = 0; i < rowsNumberInTabularData; i++)
        {
            lines.Add(CreateFileLine(GetRow(csvReportFileData.TabularData, i)));
        }

        try
        {
            var fullPath = Path.Combine(exportFolder, csvReportFileData.FileName);
            await File.AppendAllLinesAsync(fullPath, lines);
        }
        catch (DirectoryNotFoundException e)
        {
            const string message = "Directory for export not found.";
            _logger.Error(e, message);
            return Result.Fail(message);
        }
        catch (Exception e)
        {
            const string message = "Exception occured while saving report file.";
            _logger.Error(e, "An unexpected error occurred: {message}", e.Message);
            return Result.Fail(message);
        }

        _logger.Information("Report file with name {FileName} successfully created in location {FilePath}.", csvReportFileData.FileName, exportFolder);
        return Result.Ok();
    }

    private static string CreateFileLine(string[] lineItems)
    {
        return string.Join(";", lineItems);
    }
    
    private static string[] GetRow(string[,] rows, int rowNumber)
    {
        return Enumerable.Range(0, rows.GetLength(1))
            .Select(x => rows[rowNumber, x])
            .ToArray();
    }
}