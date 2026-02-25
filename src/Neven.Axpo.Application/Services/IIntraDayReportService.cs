using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IIntraDayReportService
{
    Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date);
    
    Task<Result<CsvReportFileData>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrades);
}