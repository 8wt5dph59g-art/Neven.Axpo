using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    
    /// <inheritdoc/>
    public async Task<Result<string>> ExportToCsvFileAsync(CsvReportData csvReportData, string exportPath, bool includeHeaders = true)
    {
        _logger.Information("Calling {Name}", nameof(ExportToCsvFileAsync));
        if (string.IsNullOrWhiteSpace(csvReportData.FileName))
        {
            return Result.Fail("Report file name must be defined.");
        }

        if (string.IsNullOrWhiteSpace(exportPath))
        {
            return Result.Fail("Report file path must be defined.");
        }

        var numberOfHeaderColumns = csvReportData.Headers.Length;
        if (includeHeaders && numberOfHeaderColumns == 0)
        {
            return Result.Fail("Data headers are missing.");
        }

        var columnsNumberInTabularData = csvReportData.TabularData.GetLength(1);
        if (includeHeaders && numberOfHeaderColumns != columnsNumberInTabularData)
        {
            return Result.Fail("Number of header columns does not match number of columns in tabular data.");
        }

        var lines = new List<string>();
        if (includeHeaders)
        {
            lines.Add(CreateFileLine(csvReportData.Headers));
        }

        var rowsNumberInTabularData = csvReportData.TabularData.GetLength(0);
        for (var i = 0; i < rowsNumberInTabularData; i++)
        {
            lines.Add(CreateFileLine(GetRow(csvReportData.TabularData, i)));
        }

        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        string fullPath;
        try
        {
            fullPath = Path.Combine(exportPath, csvReportData.FileName);
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

        _logger.Information("Report file with name {FileName} successfully created in location {FilePath}.", csvReportData.FileName, exportPath);
        return fullPath;
    }

    private static string CreateFileLine(string[] lineItems)
    {
        return string.Join(IntraDayCsvReportConfiguration.CsvDelimiter, lineItems);
    }
    
    private static string[] GetRow(string[,] rows, int rowNumber)
    {
        return Enumerable.Range(0, rows.GetLength(1))
            .Select(x => rows[rowNumber, x])
            .ToArray();
    }
}