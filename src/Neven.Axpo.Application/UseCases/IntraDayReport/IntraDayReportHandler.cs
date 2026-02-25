using FluentResults;
using Neven.Axpo.Application.Services;
using Serilog;

namespace Neven.Axpo.Application.UseCases.IntraDayReport;

public class IntraDayReportHandler(IIntraDayReportService intraDayReportService, IExportReportsService exportReportsService, ILogger logger)
{
    private readonly IIntraDayReportService _intraDayReportService = intraDayReportService?? throw new ArgumentNullException(nameof(intraDayReportService));
    private readonly IExportReportsService _exportReportsService = exportReportsService?? throw new ArgumentNullException(nameof(exportReportsService));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> GenerateReportAsync(DateTime date, string exportPath)
    {
        _logger.Information("Received request to generate IntraDay report with date {date}", date);
        var reportData = await _intraDayReportService.GenerateDataAsync(date);
        if (reportData.IsFailed)
        {
            return Result.Fail(reportData.Errors);
        }

        var dataForExport = await _intraDayReportService.PrepareDataForCsvExportAsync(reportData.Value);
        if (dataForExport.IsFailed)
        {
            return Result.Fail(dataForExport.Errors);
        }

        var exportResult = await _exportReportsService.ExportToCsvFileAsync(dataForExport.Value, exportPath);
        if (exportResult.IsFailed)
        {
            return Result.Fail(exportResult.Errors);
        }

        return Result.Ok();
    }
}