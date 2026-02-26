using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Neven.Axpo.Domain.Entities;
using Neven.Axpo.Infrastructure.Services;
using Neven.Axpo.UnitTests.Infrastructure;

namespace Neven.Axpo.Infrastructure.UnitTests;

public class ExportReportsServiceTests
{
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileName_Invalid(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportData.FileName = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file name must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePath_Invalid(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData,
        ExportReportsService sut)
    {
        // Arrange
        var exportFolder = string.Empty;
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Report file path must be defined.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_Missing(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportData.Headers = [];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Data headers are missing.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFileHeaders_LengthInvalid(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportData.Headers = ["header1", "header2", "header3"];
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Number of header columns does not match number of columns in tabular data.", 
            result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_ReportFilePathError_Exception(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData,
        string exportFolder,
        ExportReportsService sut)
    {
        // Arrange
        csvReportData.FileName = "/////";
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Exception occured while saving report file.", result.Errors[0].Message);
    }
    
    [Theory, AutoMoqData]
    public async Task ExportToCsvFileAsync_Ok(
        [UseCustomization(typeof(ReportFileCustomization))] CsvReportData csvReportData, ExportReportsService sut)
    {
        // Arrange
        var exportFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assert.NotNull(exportFolder);
        
        // Act
        var result = await sut.ExportToCsvFileAsync(csvReportData, exportFolder);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        Assert.True(File.Exists(result.Value));
        var lines = await File.ReadAllLinesAsync(result.Value);
        Assert.Equal(3, lines.Length);
        Assert.Equal("header1;header2", lines[0]);
        Assert.Equal("13:00;100.20", lines[1]);
        Assert.Equal("14:00;80.50", lines[2]);
    }
}