using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Axpo;
using Moq;
using Neven.Axpo.Domain.Entities;
using Neven.Axpo.Infrastructure.Services;
using Neven.Axpo.UnitTests.Infrastructure;
using Serilog;

namespace Neven.Axpo.Infrastructure.UnitTests;

public class IntraDayReportServiceTests
{
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_Throws_PowerServiceException(
        [Frozen] Mock<ILogger> logger,
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
        powerService.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new PowerServiceException("SomeError"));
        
        // Act
        var result = await sut.GenerateDataAsync(new DateTime());
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Unable to get trade data.", result.Errors[0].Message);
        logger.Verify(x => x.Error(It.IsAny<PowerServiceException>(), 
            "{ExceptionType} occured while trying to get trade data." , nameof(PowerServiceException)), Times.Once());
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_Throws_Exception(
        Exception e,
        [Frozen] Mock<ILogger> logger,
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
        powerService.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(e);
        
        // Act
        var result = await sut.GenerateDataAsync(new DateTime());
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Unable to get trade data.", result.Errors[0].Message);
        logger.Verify(x => x.Error(It.IsAny<Exception>(), 
            "An unexpected error occurred: {message}", e.Message), Times.Once());
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_ReturnEmptyArray_Result_EmptyAggregationData(
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
        var periodStartDate = new DateTime(2026, 2, 1, 23, 0, 0);
        var date =  new DateTime(2026, 2, 2, 15, 0, 0);
        powerService.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([]);
        
        // Act
        var result = await sut.GenerateDataAsync(date);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Aggregations);
        Assert.Equal(24, result.Value.Aggregations.Length);
        for(var i = 0; i < 24; i++)
        {
            Assert.Equal( periodStartDate.AddHours(i), result.Value.Aggregations[i].Period);
            Assert.Null(result.Value.Aggregations[i].AggregatedVolume);
        }
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_Result_Correct(
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
        var periodStartDate = new DateTime(2026, 2, 1, 23, 0, 0);
        var date = new DateTime(2026,2,2,15,15,15);
        var trades = new List<PowerTrade>
        {
            PowerTrade.Create(date, 4),
            PowerTrade.Create(date, 4)
        };
        
        for (var i = 0; i < 4; i++)
        {
            trades[0].Periods[i].SetVolume(trades[0].Periods[i].Period);
            trades[1].Periods[i].SetVolume(trades[1].Periods[i].Period);
        }
        
        powerService.Setup(x => x.GetTradesAsync(date))
            .ReturnsAsync(trades);
        
        // Act
        var result = await sut.GenerateDataAsync(date);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Aggregations);
        Assert.Equal(24, result.Value.Aggregations.Length);
        Assert.Equal(periodStartDate, result.Value.Aggregations[0].Period);
        Assert.Equal(2d, result.Value.Aggregations[0].AggregatedVolume);
        Assert.Equal(periodStartDate.AddHours(1), result.Value.Aggregations[1].Period);
        Assert.Equal(4d, result.Value.Aggregations[1].AggregatedVolume);
        Assert.Equal(periodStartDate.AddHours(2), result.Value.Aggregations[2].Period);
        Assert.Equal(6d, result.Value.Aggregations[2].AggregatedVolume);
        Assert.Equal(periodStartDate.AddHours(3), result.Value.Aggregations[3].Period);
        Assert.Equal(8d, result.Value.Aggregations[3].AggregatedVolume);
        for(var i = 4; i < 24; i++)
        {
            Assert.Equal(periodStartDate.AddHours(i), result.Value.Aggregations[i].Period);
            Assert.Null(result.Value.Aggregations[i].AggregatedVolume);
        }
    }
    
    [Theory, AutoMoqData]
    public async Task PrepareDataForCsvExportAsync_Ok(
        [UseCustomization(typeof(AggregatedPowerTradeCustomization))] AggregatedPowerTrade aggregatedPowerTrade,
        IntraDayReportService sut)
    {
        // Arrange
        
        // Act
        var result = await sut.PrepareDataForCsvExportAsync(aggregatedPowerTrade);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"PowerPosition_{aggregatedPowerTrade.TimeStamp:yyyyMMdd}_{aggregatedPowerTrade.TimeStamp:HHmm}.csv", result.Value.FileName);
        Assert.Contains(IntraDayCsvReportConfiguration.HeaderLocalTime, result.Value.Headers);
        Assert.Contains(IntraDayCsvReportConfiguration.HeaderVolume, result.Value.Headers);
        Assert.Equal("23:00", result.Value.TabularData[0,0]);
        Assert.Equal("100", result.Value.TabularData[0,1]);
        Assert.Equal("00:00", result.Value.TabularData[1,0]);
        Assert.Equal("200", result.Value.TabularData[1,1]);
        Assert.Equal("01:00", result.Value.TabularData[2,0]);
        Assert.Equal("200.06622", result.Value.TabularData[2,1]);
        Assert.Equal("02:00", result.Value.TabularData[3,0]);
        Assert.Equal("-75.06622", result.Value.TabularData[3,1]);
        for (var i = 4; i < 16; i++)
        {
            Assert.Equal((i-1).ToString("00") + ":00", result.Value.TabularData[i,0]);
            Assert.Equal("Data Not Available", result.Value.TabularData[i,1]);
        }
    }
    
    [Theory, AutoMoqData]
    public async Task PrepareDataForCsvExportAsync_AggregatePowerTrade_No_Periods_Returs_Error(
        [UseCustomization(typeof(AggregatedPowerTradeCustomization))] AggregatedPowerTrade aggregatedPowerTrade,
        IntraDayReportService sut)
    {
        // Arrange
        aggregatedPowerTrade.Aggregations = [];
        
        // Act
        var result = await sut.PrepareDataForCsvExportAsync(aggregatedPowerTrade);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Aggregation data is missing.", result.Errors[0].Message);
    }
}