using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

/// <summary>
/// This interface defines methods used to export report data into different formats.
/// </summary>
public interface IExportReportsService
{
    /// <summary>
    /// Thgis method is used to export report data to CSV file.
    /// </summary>
    /// <param name="csvReportData">Data to export.</param>
    /// <param name="exportFolder">Location where file is supposed to be saved.</param>
    /// <param name="includeHeaders">Optional parameter specifying do we want to include headers.</param>
    /// <returns>Returns result that indicates was export successful or not.</returns>
    Task<Result> ExportToCsvFileAsync(CsvReportData csvReportData, string exportFolder, bool includeHeaders = true);
}