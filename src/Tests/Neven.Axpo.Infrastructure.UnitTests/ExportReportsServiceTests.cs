using System.Reflection;
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
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportFileData.FileName = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file name must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePath_Invalid(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        ExportReportsService sut)
    {
        // Arrange
        var exportFolder = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file path must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_Missing(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportFileData.Headers = [];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Data headers are missing.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_LengthInvalid(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportFileData.Headers = ["header1", "header2", "header3"];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Number of header columns does not match number of columns in tabular data.", 
            result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePathError_DirectoryNotFoundException(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        [Frozen] Mock<ILogger> logger, ExportReportsService sut)
    {
        // Arrange
        var exportFolder = "aaaa";
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Directory for export not found.", result.Errors[0].Message);
        logger.Verify(x => x.Error(It.IsAny<DirectoryNotFoundException>(), 
            "Directory for export not found."), Times.Once());
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePathError_Exception(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportFileData.FileName = "/////";
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Exception occured while saving report file.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_Ok(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportFileData csvReportFileData,
        [Frozen] Mock<ILogger> logger, ExportReportsService sut)
    {
        // Arrange
        var exportFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assert.NotNull(exportFolder);
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportFileData, exportFolder);
        
        // Assert
        Assert.True(result.IsSuccess);
        logger.Verify(x => x.Information(
            "Report file with name {FileName} successfully created in location {FilePath}.", 
            csvReportFileData.FileName, exportFolder), Times.Once());

        var filePath = Path.Combine(exportFolder, csvReportFileData.FileName);
        Assert.True(File.Exists(filePath));
        var lines = await File.ReadAllLinesAsync(filePath);
        Assert.Equal(3, lines.Length);
        Assert.Equal("header1;header2", lines[0]);
        Assert.Equal("13:00;100.20", lines[1]);
        Assert.Equal("14:00;80.50", lines[2]);
    }
}