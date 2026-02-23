using Axpo;
using FluentResults;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;
using Serilog;

namespace Neven.Axpo.Infrastructure.Services;

public class IntraDayReportService(IPowerService powerService, ILogger logger) : IIntraDayReportService
{
    private readonly IPowerService _powerService = powerService?? throw new ArgumentNullException(nameof(powerService));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    public Task<Result<AggregatedPowerTrades>> GenerateDataAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ReportFile>> PrepareDataForExportAsync(AggregatedPowerTrades aggregatedPowerTrades)
    {
        throw new NotImplementedException();
    }
}