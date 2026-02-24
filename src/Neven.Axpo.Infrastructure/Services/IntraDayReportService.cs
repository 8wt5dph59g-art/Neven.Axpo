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

    public Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ReportFile>> PrepareDataForExportAsync(AggregatedPowerTrade aggregatedPowerTrade, string exportPath)
    {
        if (aggregatedPowerTrade.Aggregations.Length == 0)
        {
            return Task.FromResult<Result<ReportFile>>(Result.Fail("There is no Data"));
        }
        
        var result = new ReportFile
        {
            FilePath = exportPath,
            FileName = $"PowerPosition_{aggregatedPowerTrade.TimeStamp:YYYYMMDD}_{aggregatedPowerTrade.TimeStamp:HHMM}.csv",
            Headers = ["Local Time", "Volume"],
            TabularData = new string[aggregatedPowerTrade.Aggregations.Length, 2]
        };
        
        for (var i = 0; i<= aggregatedPowerTrade.Aggregations.Length; i++)
        {
            var aggregatedPowerPeriod = aggregatedPowerTrade.Aggregations[i];
            result.TabularData[i, 0] = TransformAggregatedPowerPeriod(aggregatedPowerPeriod.Period);
            result.TabularData[i, 1] = aggregatedPowerTrade.Aggregations[i].AggregatedVolume.ToString("N");
        }

        return Task.FromResult(Result.Ok(result));
    }

    private static string TransformAggregatedPowerPeriod(int period)
    {
        var x = 23;
        if (period > 1)
        {
            x = period - 2;
        }
        
        return x.ToString("00") + ":00";
    }
}