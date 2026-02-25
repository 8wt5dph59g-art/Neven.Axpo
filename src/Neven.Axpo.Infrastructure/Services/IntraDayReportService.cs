using System.Globalization;
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

        var periodMatching = GeneratePeriodMatching(date);
        
        var result = new AggregatedPowerTrade
        {
            TimeStamp = date,
            Aggregations = volumeByPeriod.Select(x => new AggregatedPowerPeriod
            {
                AggregatedVolume = x.Value, 
                Period = periodMatching[x.Key]
            }).ToArray()
        };

        return Result.Ok(result);
    }

    public Task<Result<CsvReportFileData>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrade)
    {
        var aggregations = aggregatedPowerTrade.Aggregations;
        var aggregationsLength = aggregations.Length;
        
        var result = new CsvReportFileData
        {
            FileName = $"PowerPosition_{aggregatedPowerTrade.TimeStamp:YYYYMMDD}_{aggregatedPowerTrade.TimeStamp:HHMM}.csv",
            Headers = [IntraDayReportHeaderData.LocalTime, IntraDayReportHeaderData.Volume],
            TabularData = new string[aggregationsLength, 2]
        };
        
        for (var i = 0; i < aggregationsLength; i++)
        {
            var aggregation = aggregations[i];
            result.TabularData[i, 0] = aggregation.Period.ToString("HH:mm");
            result.TabularData[i, 1] = aggregation.AggregatedVolume.ToString(CultureInfo.InvariantCulture);
        }

        return Task.FromResult(Result.Ok(result));
    }

    private static Dictionary<int, DateTime> GeneratePeriodMatching(DateTime reportDateTime)
    {
        var gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var result = new Dictionary<int, DateTime>();
        
        var reportDateTimeLowerBound = new DateTime(reportDateTime.Year, reportDateTime.Month, reportDateTime.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
        var reportDateTimeUpperBound = reportDateTimeLowerBound.AddDays(1.0);
        var reportDateTimeLowerBoundUtc = TimeZoneInfo.ConvertTimeToUtc(reportDateTimeLowerBound, gmtTimeZoneInfo);
        var reportDateTimeUpperBoundUtc = TimeZoneInfo.ConvertTimeToUtc(reportDateTimeUpperBound, gmtTimeZoneInfo);
        var numberOfPeriods = (int) reportDateTimeUpperBoundUtc.Subtract(reportDateTimeLowerBoundUtc).TotalHours;
        for (var i = 0; i < numberOfPeriods; i++)
        {
            result.Add(i+1, reportDateTimeLowerBoundUtc.AddHours(i));
        }

        return result;
    }
}