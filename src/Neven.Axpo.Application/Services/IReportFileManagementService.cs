using FluentResults;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Application.Services;

public interface IReportFileManagementService
{
    Task<Result> ExportToCsvFileAsync(ReportFile reportFile);
}