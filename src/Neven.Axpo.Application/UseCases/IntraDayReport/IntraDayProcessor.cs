using FluentResults;
using Neven.Axpo.Application.Services;
using Serilog;

namespace Neven.Axpo.Application.UseCases.IntraDayReport;

public class IntraDayProcessor(IIntraDayReportService intraDayReportService, IReportFileManagementService reportFileManagementService, ILogger logger)
{
    private readonly IIntraDayReportService _intraDayReportService = intraDayReportService?? throw new ArgumentNullException(nameof(intraDayReportService));
    private readonly IReportFileManagementService _reportFileManagementService = reportFileManagementService?? throw new ArgumentNullException(nameof(reportFileManagementService));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> GenerateReportAsync(DateTime date, string exportPath)
    {
        _logger.Information("Received request to generate IntraDay report with date {date}", date);
        var reportData = await _intraDayReportService.GenerateDataAsync(date);
        if (reportData.IsFailed)
        {
            return Result.Fail(reportData.Errors);
        }

        var dataForExport = await _intraDayReportService.PrepareDataForExportAsync(reportData.Value, exportPath);
        if (dataForExport.IsFailed)
        {
            return Result.Fail(dataForExport.Errors);
        }

        var exportResult = await _reportFileManagementService.ExportToCsvFileAsync(dataForExport.Value);
        if (exportResult.IsFailed)
        {
            return Result.Fail(exportResult.Errors);
        }

        return Result.Ok();
    }
}