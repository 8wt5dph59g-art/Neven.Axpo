using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

/// <summary>
/// This interface defines methods used to generate Power Trades Intra-Day report.
/// </summary>
public interface IIntraDayReportService
{
    /// <summary>
    /// This method is used to Generate Intra-Day report data.
    /// </summary>
    /// <param name="date">Report date.</param>
    /// <returns>Returns aggregated trade volumes by periods.</returns>
    Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date);
    
    /// <summary>
    /// This method prepares aggregated trade volumes by periods for export to CSV file.
    /// </summary>
    /// <param name="aggregatedPowerTrades">Aggregated power trades.</param>
    /// <returns>Returns aggegated power trades report data that is ready for CSV export.</returns>
    Task<Result<CsvReportData>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrades);
}