using FluentResults;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;

namespace Neven.Axpo.Infrastructure.Services;

public class ReportFileManagementService : IReportFileManagementService
{
    public Task<Result> ExportToCsvFileAsync(ReportFile reportFile)
    {
        throw new NotImplementedException();
    }
}