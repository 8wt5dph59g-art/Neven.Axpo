using System;
using System.Threading.Tasks;
using FluentResults;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;
using Serilog;

namespace Neven.Axpo.Application.UseCases.IntraDayReport;

/// <summary>
/// Intra-Day report handler.
/// </summary>
/// <param name="intraDayReportService">Instance of Intra-Day Report service.</param>
/// <param name="exportReportsService">Instance of Export Reports service.</param>
/// <param name="logger">Logger</param>
public class IntraDayReportHandler(
    IIntraDayReportService intraDayReportService,
    IExportReportsService exportReportsService,
    IDateTimeProvider dateTimeProvider,
    ILogger logger)
{
    private readonly IIntraDayReportService _intraDayReportService = intraDayReportService?? throw new ArgumentNullException(nameof(intraDayReportService));
    private readonly IExportReportsService _exportReportsService = exportReportsService?? throw new ArgumentNullException(nameof(exportReportsService));
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider?? throw new ArgumentNullException(nameof(dateTimeProvider));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// This method will generate Intra-Day power trades report and export it to CSV file.
    /// </summary>
    /// <param name="exportPath">Export destination.</param>
    /// <returns>Returns result that indicates was operation successful or not.</returns>
    public async Task<Result> GenerateCsvReportAsync(string exportPath)
    {
        Result<AggregatedPowerTrade> reportData;
        try
        {
            var date = _dateTimeProvider.GetCurrentLocalTime();
            _logger.Information("Getting report for local date time {LocalDateTime}.", date);
            reportData = await _intraDayReportService.GenerateDataAsync(date);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unhandled exception occured while trying to generate report data.");
            return Result.Fail("Failed to generate report data.");
        }
        
        if (reportData.IsFailed)
        {
            return Result.Fail(reportData.Errors);
        }

        Result<CsvReportData> dataForExport;
        try
        {
            dataForExport = await _intraDayReportService.PrepareDataForCsvExportAsync(reportData.Value);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unhandled exception occured while trying to prepare report data for CSV export.");
            return Result.Fail("Failed to prepare report data for CSV export.");
        }
        
        if (dataForExport.IsFailed)
        {
            return Result.Fail(dataForExport.Errors);
        }

        Result<string> exportResult;
        try
        {
            exportResult = await _exportReportsService.ExportToCsvFileAsync(dataForExport.Value, exportPath);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unhandled exception occured while trying to save report to CSV file.");
            return Result.Fail("Failed to save report to CSV file.");
        }
        
        _logger.Information("Report file successfully created {FullFilePath}.", exportResult.Value);
        
        return exportResult.IsFailed ? Result.Fail(exportResult.Errors) : Result.Ok();
    }
}