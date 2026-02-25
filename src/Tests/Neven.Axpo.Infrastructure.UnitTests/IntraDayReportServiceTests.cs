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
        Assert.Equal("Unable to get trade data", result.Errors[0].Message);
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
        Assert.Equal("Unable to get trade data", result.Errors[0].Message);
        logger.Verify(x => x.Error(It.IsAny<Exception>(), 
            "An unexpected error occurred: {message}", e.Message), Times.Once());
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_ReturnEmptyArray_Result_Empty(
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
        powerService.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([]);
        
        // Act
        var result = await sut.GenerateDataAsync(DateTime.Now);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Aggregations);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateDataAsync_PowerService_ReturnEmptyArray_Result_Correct(
        [Frozen] Mock<IPowerService> powerService,
        IntraDayReportService sut)
    {
        // Arrange
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
        Assert.Equal(new DateTime(2026, 2, 1, 23, 0, 0), result.Value.Aggregations[0].Period);
        Assert.Equal(2d, result.Value.Aggregations[0].AggregatedVolume);
        Assert.Equal(new DateTime(2026, 2, 2, 0, 0, 0), result.Value.Aggregations[1].Period);
        Assert.Equal(4d, result.Value.Aggregations[1].AggregatedVolume);
        Assert.Equal(new DateTime(2026, 2, 2, 1, 0, 0), result.Value.Aggregations[2].Period);
        Assert.Equal(6d, result.Value.Aggregations[2].AggregatedVolume);
        Assert.Equal(new DateTime(2026, 2, 2, 2, 0, 0), result.Value.Aggregations[3].Period);
        Assert.Equal(8d, result.Value.Aggregations[3].AggregatedVolume);
    }
    
    [Theory, AutoMoqData]
    public async Task PrepareDataForCsvExportAsync_Ok(
        [UseCustomization(typeof(AggregatedPowerTradeCustomization))] AggregatedPowerTrade aggregatedPowerTrade,
        string exportPath,
        IntraDayReportService sut)
    {
        // Arrange
        
        // Act
        var result = await sut.PrepareDataForCsvExportAsync(aggregatedPowerTrade, exportPath);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(exportPath, result.Value.FilePath);
        Assert.Equal($"PowerPosition_{aggregatedPowerTrade.TimeStamp:YYYYMMDD}_{aggregatedPowerTrade.TimeStamp:HHMM}.csv", result.Value.FileName);
        Assert.Contains(IntraDayReportHeaderData.LocalTime, result.Value.Headers);
        Assert.Contains(IntraDayReportHeaderData.Volume, result.Value.Headers);
        Assert.Equal("23:00", result.Value.TabularData[0,0]);
        Assert.Equal("100", result.Value.TabularData[0,1]);
        Assert.Equal("00:00", result.Value.TabularData[1,0]);
        Assert.Equal("200", result.Value.TabularData[1,1]);
        Assert.Equal("03:00", result.Value.TabularData[2,0]);
        Assert.Equal("200.06622", result.Value.TabularData[2,1]);
        Assert.Equal("18:00", result.Value.TabularData[3,0]);
        Assert.Equal("-75.06622", result.Value.TabularData[3,1]);
    }
    
    [Theory, AutoMoqData]
    public async Task PrepareDataForCsvExportAsync_AggregatePowerTrade_No_Periods(
        [UseCustomization(typeof(AggregatedPowerTradeCustomization))] AggregatedPowerTrade aggregatedPowerTrade,
        string exportPath,
        IntraDayReportService sut)
    {
        // Arrange
        aggregatedPowerTrade.Aggregations = [];
        
        // Act
        var result = await sut.PrepareDataForCsvExportAsync(aggregatedPowerTrade, exportPath);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(exportPath, result.Value.FilePath);
        Assert.Equal($"PowerPosition_{aggregatedPowerTrade.TimeStamp:YYYYMMDD}_{aggregatedPowerTrade.TimeStamp:HHMM}.csv", result.Value.FileName);
        Assert.Contains(IntraDayReportHeaderData.LocalTime, result.Value.Headers);
        Assert.Contains(IntraDayReportHeaderData.Volume, result.Value.Headers);
        Assert.Empty(result.Value.TabularData);
    }
}