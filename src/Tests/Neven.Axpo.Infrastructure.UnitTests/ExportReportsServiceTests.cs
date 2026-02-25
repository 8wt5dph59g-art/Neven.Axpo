using AutoFixture.Xunit2;
using Moq;
using Neven.Axpo.Domain.Entities;
using Neven.Axpo.Infrastructure.Services;
using Neven.Axpo.UnitTests.Infrastructure;
using Serilog;

namespace Neven.Axpo.Infrastructure.UnitTests;

public class ExportReportsServiceTests
{
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileName_Invalid(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        ExportReportsService sut)
    {
        // Arrange
        reportFile.FileName = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file name must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePath_Invalid(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        ExportReportsService sut)
    {
        // Arrange
        reportFile.FilePath = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file path must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_Missing(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        ExportReportsService sut)
    {
        // Arrange
        reportFile.Headers = [];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Data headers are missing.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_LengthInvalid(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        ExportReportsService sut)
    {
        // Arrange
        reportFile.Headers = ["header1", "header2", "header3"];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Number of header columns does not match number of columns in tabular data.", 
            result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePathError_DirectoryNotFoundException(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        [Frozen] Mock<ILogger> logger, ExportReportsService sut)
    {
        // Arrange
        reportFile.FilePath = "aaaa";
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Directory for export not found.", result.Errors[0].Message);
        logger.Verify(x => x.Error(It.IsAny<DirectoryNotFoundException>(), 
            "Directory for export not found."), Times.Once());
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePathError_Exception(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        ExportReportsService sut)
    {
        // Arrange
        reportFile.FileName = "/////";
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Exception occured while saving report file.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_Ok(
        [UseCustomization(typeof(ReportFileCustomization))] ReportFile reportFile,
        [Frozen] Mock<ILogger> logger, ExportReportsService sut)
    {
        // Arrange
        
        // Act
        var result = await sut.ExportToCsvFileAsync(reportFile);
        
        // Assert
        Assert.True(result.IsSuccess);
        logger.Verify(x => x.Information(
            "Report file with name {FileName} successfully created in location {FilePath}.", 
            reportFile.FileName, reportFile.FilePath), Times.Once());

        var filePath = Path.Combine(reportFile.FilePath, reportFile.FileName);
        Assert.True(File.Exists(filePath));
        var lines = await File.ReadAllLinesAsync(filePath);
        Assert.Equal(3, lines.Length);
        Assert.Equal("header1;header2", lines[0]);
        Assert.Equal("13:00;100.20", lines[1]);
        Assert.Equal("14:00;80.50", lines[2]);
    }
}