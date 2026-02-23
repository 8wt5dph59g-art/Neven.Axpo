using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IIntraDayReportService
{
    Task<Result<AggregatedPowerTrades>> GenerateDataAsync(DateTime date);
    Task<Result<ReportFile>> PrepareDataForExportAsync(AggregatedPowerTrades aggregatedPowerTrades);
}