using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IExportReportsService
{
    Task<Result> ExportToCsvFileAsync(CsvReportData csvReportData, string exportFolder, bool includeHeaders = true);
}