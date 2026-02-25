using AutoFixture.Xunit2;
using FluentResults;
using Moq;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Application.UseCases.IntraDayReport;
using Neven.Axpo.Domain.Entities;
using Neven.Axpo.UnitTests.Infrastructure;
using Serilog;

namespace Neven.Axpo.Application.UnitTests;

public class IntraDayReportHandlerTests
{
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_GenerateDataAsync_ThrowsException(
        Exception exception,
        DateTime reportDate,
        [Frozen] Mock<ILogger> logger,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        IntraDayReportHandler sut)
    {
        // Arrange
        intraDayReportService.Setup(x => 
            x.GenerateDataAsync(It.IsAny<DateTime>())).ThrowsAsync(exception);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Failed to generate report data.", result.Errors[0].Message);
        logger.Verify(x => x.Error(exception, 
            "Unhandled exception occured while trying to generate report data."), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_GenerateDataAsync_ReturnsFailure(
        DateTime reportDate,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        IntraDayReportHandler sut)
    {
        // Arrange
        var trades = Result.Fail("Failed to generate report data.");
        intraDayReportService.Setup(x => 
            x.GenerateDataAsync(It.IsAny<DateTime>())).ReturnsAsync(trades);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Failed to generate report data.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_PrepareDataForCsvExportAsync_ThrowsException(
        Exception exception,
        DateTime reportDate,
        [Frozen] Mock<ILogger> logger,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        IntraDayReportHandler sut)
    {
        // Arrange
        intraDayReportService.Setup(x => 
            x.GenerateDataAsync(It.IsAny<DateTime>())).ReturnsAsync(new Result<AggregatedPowerTrade>());
        
        intraDayReportService.Setup(x => 
            x.PrepareDataForCsvExportAsync(It.IsAny<AggregatedPowerTrade>())).ThrowsAsync(exception);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Failed to prepare report data for CSV export.", result.Errors[0].Message);
        logger.Verify(x => x.Error(exception, 
            "Unhandled exception occured while trying to prepare report data for CSV export."), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_PrepareDataForCsvExportAsync_ReturnsFailure(
        DateTime reportDate,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        IntraDayReportHandler sut)
    {
        // Arrange
        var csvReportFileData = Result.Fail("Some error.");
        
        intraDayReportService.Setup(x => 
            x.GenerateDataAsync(It.IsAny<DateTime>())).ReturnsAsync(new Result<AggregatedPowerTrade>());
        
        intraDayReportService.Setup(x => 
            x.PrepareDataForCsvExportAsync(It.IsAny<AggregatedPowerTrade>())).ReturnsAsync(csvReportFileData);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Some error.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_ExportToCsvFileAsync_ThrowsException(
        Exception exception,
        DateTime reportDate,
        [Frozen] Mock<ILogger> logger,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        [Frozen] Mock<IExportReportsService> exportReportsService,
        IntraDayReportHandler sut)
    {
        // Arrange
        intraDayReportService.Setup(x => 
            x.GenerateDataAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new Result<AggregatedPowerTrade>());
        
        intraDayReportService.Setup(x => 
            x.PrepareDataForCsvExportAsync(It.IsAny<AggregatedPowerTrade>()))
            .ReturnsAsync(new Result<CsvReportData>());

        exportReportsService.Setup(x => 
            x.ExportToCsvFileAsync(It.IsAny<CsvReportData>(), It.IsAny<string>()))
            .ThrowsAsync(exception);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Failed to save report to CSV file.", result.Errors[0].Message);
        logger.Verify(x => x.Error(exception, 
            "Unhandled exception occured while trying to save report to CSV file."), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_ExportToCsvFileAsync_ReturnsFailure(
        DateTime reportDate,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        [Frozen] Mock<IExportReportsService> exportReportsService,
        IntraDayReportHandler sut)
    {
        // Arrange
        var exportFailure = Result.Fail("Some error.");
        
        intraDayReportService.Setup(x => 
                x.GenerateDataAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new Result<AggregatedPowerTrade>());
        
        intraDayReportService.Setup(x => 
                x.PrepareDataForCsvExportAsync(It.IsAny<AggregatedPowerTrade>()))
            .ReturnsAsync(new Result<CsvReportData>());

        exportReportsService.Setup(x => 
                x.ExportToCsvFileAsync(It.IsAny<CsvReportData>(), It.IsAny<string>()))
            .ReturnsAsync(exportFailure);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Some error.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateCsvReportAsync_ExportToCsvFileAsync_Ok(
        DateTime reportDate,
        [Frozen] Mock<IIntraDayReportService> intraDayReportService,
        [Frozen] Mock<IExportReportsService> exportReportsService,
        IntraDayReportHandler sut)
    {
        // Arrange
        var exportFailure = Result.Fail("Some error.");
        
        intraDayReportService.Setup(x => 
                x.GenerateDataAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new Result<AggregatedPowerTrade>());
        
        intraDayReportService.Setup(x => 
                x.PrepareDataForCsvExportAsync(It.IsAny<AggregatedPowerTrade>()))
            .ReturnsAsync(new Result<CsvReportData>());

        exportReportsService.Setup(x => 
                x.ExportToCsvFileAsync(It.IsAny<CsvReportData>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok);
        
        // Act
        var result = await sut.GenerateCsvReportAsync(reportDate, string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
    }
}