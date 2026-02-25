using Axpo;
using FluentResults;
using JetBrains.Annotations;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;
using Serilog;

namespace Neven.Axpo.Infrastructure.Services;

[UsedImplicitly]
public class IntraDayReportService(IPowerService powerService, ILogger logger) : IIntraDayReportService
{
    private readonly IPowerService _powerService = powerService?? throw new ArgumentNullException(nameof(powerService));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date)
    {
        IEnumerable<PowerTrade> powerTrades;

        try
        {
            powerTrades = await _powerService.GetTradesAsync(date);
        }
        catch (PowerServiceException e)
        {
            _logger.Error(e, "{ExceptionType} occured while trying to get trade data." , nameof(PowerServiceException));
            return Result.Fail("Unable to get trade data");
        }
        catch (Exception e)
        {
            _logger.Error(e, "An unexpected error occurred: {message}", e.Message);
            return Result.Fail("Unable to get trade data");
        }

        var volumeByPeriod = powerTrades
            .SelectMany(t => t.Periods)
            .GroupBy(p => p.Period)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(p => p.Volume)
            );

        var result = new AggregatedPowerTrade
        {
            TimeStamp = date,
            Aggregations = volumeByPeriod.Select(x => new AggregatedPowerPeriod
            {
                AggregatedVolume = x.Value, 
                Period = x.Key
            }).ToArray()
        };

        return Result.Ok(result);
    }

    public Task<Result<ReportFile>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrade, string exportPath)
    {
        var aggregations = aggregatedPowerTrade.Aggregations;
        var aggregationsLength = aggregations.Length;
        
        var result = new ReportFile
        {
            FilePath = exportPath,
            FileName = $"PowerPosition_{aggregatedPowerTrade.TimeStamp:YYYYMMDD}_{aggregatedPowerTrade.TimeStamp:HHMM}.csv",
            Headers = [IntraDayReportHeaderData.LocalTime, IntraDayReportHeaderData.Volume],
            TabularData = new string[aggregationsLength, 2]
        };
        
        for (var i = 0; i < aggregationsLength; i++)
        {
            var aggregation = aggregations[i];
            result.TabularData[i, 0] = TransformAggregatedPowerPeriod(aggregation.Period);
            result.TabularData[i, 1] = aggregation.AggregatedVolume.ToString("N2");
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