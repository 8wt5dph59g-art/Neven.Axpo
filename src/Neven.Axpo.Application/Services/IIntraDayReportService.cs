using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IIntraDayReportService
{
    Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date);
    
    Task<Result<CsvReportData>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrades);
}