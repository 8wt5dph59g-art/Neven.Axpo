using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

    /// <inheritdoc/>
    public async Task<Result<AggregatedPowerTrade>> GenerateDataAsync(DateTime date)
    {
        _logger.Debug("Calling {MethodName}", nameof(GenerateDataAsync));
        _logger.Debug("Parameter date has value {Value}", date);
        
        List<PowerTrade> powerTrades;
        try
        {
            powerTrades = (await _powerService.GetTradesAsync(date)).ToList();
            
            _logger.Debug("{ServiceName} returned following trades {@Value}", nameof(PowerService), powerTrades);
        }
        catch (PowerServiceException e)
        {
            _logger.Error(e, "{ExceptionType} occured while trying to get trade data." , 
                nameof(PowerServiceException));
            return Result.Fail("Unable to get trade data.");
        }
        catch (Exception e)
        {
            _logger.Error(e, "An unexpected error occurred: {message}", e.Message);
            return Result.Fail("Unable to get trade data.");
        }

        Dictionary<int, double> volumeByPeriod;
        try
        {
            volumeByPeriod = powerTrades
                .SelectMany(t => t.Periods)
                .GroupBy(p => p.Period)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.Volume)
                );
            
            _logger.Debug("Volumes grouped by periods {@Value}", volumeByPeriod);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception occured while trying to group power trade volumes.");
            return Result.Fail("Unable to group power trade volumes.");
        }

        Dictionary<int, Tuple<DateTime, double?>> reportStructure;
        try
        {
            reportStructure = GenerateReportStructure(date);
            
            _logger.Debug("Initial report structure that contains all periods and default volumes {@Value}", reportStructure);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception occured while trying to generate initial report structure.");
            return Result.Fail("Unable to generate initial report structure.");
        }

        AggregatedPowerTrade result;
        try
        {
            result = new AggregatedPowerTrade
            {
                TimeStamp = date,
                Aggregations = reportStructure.Select(x => new AggregatedPowerPeriod
                {
                    AggregatedVolume = volumeByPeriod.TryGetValue(x.Key, out var value) ? value : null, 
                    Period = reportStructure[x.Key].Item1
                }).ToArray()
            };
            
            _logger.Debug("Aggregated trades merged with initial report structure {@Value}", result);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception occured while trying to generate final report structure.");
            return Result.Fail("Unable to generate final report structure.");
        }

        return Result.Ok(result);
    }

    /// <inheritdoc/>
    public Task<Result<CsvReportData>> PrepareDataForCsvExportAsync(AggregatedPowerTrade aggregatedPowerTrade)
    {
        _logger.Debug("Calling {MethodName}", nameof(PrepareDataForCsvExportAsync));
        
        var aggregations = aggregatedPowerTrade.Aggregations;
        var aggregationsLength = aggregations.Length;

        if (aggregationsLength == 0)
        {
            return Task.FromResult<Result<CsvReportData>>(Result.Fail("Aggregation data is missing."));
        }

        CsvReportData result;
        try
        {
            result = new CsvReportData
            {
                FileName = $"PowerPosition_{aggregatedPowerTrade.TimeStamp:yyyyMMdd}_{aggregatedPowerTrade.TimeStamp:HHmm}.csv",
                Headers = [IntraDayCsvReportConfiguration.HeaderLocalTime, IntraDayCsvReportConfiguration.HeaderVolume],
                TabularData = new string[aggregationsLength, 2]
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception occured while trying to initiate instance of {Name}.", nameof(CsvReportData));
            return Task.FromResult<Result<CsvReportData>>(Result.Fail($"Exception occured while trying to initiate instance of {nameof(CsvReportData)}."));
        }

        try
        {
            for (var i = 0; i < aggregationsLength; i++)
            {
                var aggregation = aggregations[i];
                result.TabularData[i, 0] = aggregation.Period.ToString("HH:mm");
                result.TabularData[i, 1] = aggregation.AggregatedVolume.HasValue 
                    ? aggregation.AggregatedVolume.Value.ToString(CultureInfo.InvariantCulture) 
                    : "Data Not Available";
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception occured while trying to prepare data for {Name}.", nameof(CsvReportData));
            return Task.FromResult<Result<CsvReportData>>(Result.Fail($"Exception occured while trying to prepare data for {nameof(CsvReportData)}."));
        }
        
        return Task.FromResult(Result.Ok(result));
    }

    private static Dictionary<int, Tuple<DateTime,double?>> GenerateReportStructure(DateTime reportDateTime)
    {
        var result = new Dictionary<int, Tuple<DateTime,double?>>();
        var (reportDateTimeLowerBoundUtc, reportDateTimeUpperBoundUtc) = DetermineReportDateTimeBounds(reportDateTime);
        var numberOfPeriods = (int) reportDateTimeUpperBoundUtc.Subtract(reportDateTimeLowerBoundUtc).TotalHours;
        for (var i = 0; i < numberOfPeriods; i++)
        {
            result.Add(i+1, new Tuple<DateTime, double?>(reportDateTimeLowerBoundUtc.AddHours(i), null));
        }

        return result;
    }

    private static Tuple<DateTime, DateTime> DetermineReportDateTimeBounds(DateTime reportDateTime)
    {
        var reportDateTimeLowerBound = new DateTime(reportDateTime.Year, reportDateTime.Month, 
            reportDateTime.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
        var reportDateTimeUpperBound = reportDateTimeLowerBound.AddDays(1.0);

        return new Tuple<DateTime, DateTime>(reportDateTimeLowerBound, reportDateTimeUpperBound);
    }
}