using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IExportReportsService
{
    Task<Result> ExportToCsvFileAsync(CsvReportFileData csvReportFileData, string exportFolder, bool includeHeaders = true);
}