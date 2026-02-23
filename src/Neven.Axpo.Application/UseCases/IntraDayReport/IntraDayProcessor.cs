using FluentResults;
using Neven.Axpo.Application.Services;
using Serilog;

namespace Neven.Axpo.Application.UseCases.IntraDayReport;

public class IntraDayProcessor(IIntraDayReportService intraDayReportService, IReportFileManagementService reportFileManagementService, ILogger logger)
{
    private readonly IIntraDayReportService _intraDayReportService = intraDayReportService?? throw new ArgumentNullException(nameof(intraDayReportService));
    private readonly IReportFileManagementService _reportFileManagementService = reportFileManagementService?? throw new ArgumentNullException(nameof(reportFileManagementService));
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));

    public Task<Result> GenerateReport(DateTime date)
    {
        throw new NotImplementedException();
    }
}