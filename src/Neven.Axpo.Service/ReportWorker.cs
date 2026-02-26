using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Application.UseCases.IntraDayReport;
using Neven.Axpo.Domain.Entities;
using Serilog;

namespace Neven.Axpo.Service;

public class ReportWorker(IServiceProvider services, ILogger logger, PowerPositionsExportSettings exportSettings) : IHostedService
{
    private readonly ILogger _logger = logger?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider _services = 
        services ?? throw new ArgumentNullException(nameof(services));
    private readonly PowerPositionsExportSettings _exportSettings =
        exportSettings ?? throw new ArgumentNullException(nameof(exportSettings));
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Starting {WorkerName} at {CurrentTime}.", nameof(ReportWorker), DateTime.Now);
        _logger.Information("Report export path is {Path}", _exportSettings.ReportPath);
        _logger.Information("Report export interval delay in minutes is {Interval}", _exportSettings.IntervalInMinutes);
        await DoWork();
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_exportSettings.IntervalInMinutes));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await DoWork();
        }
    }
    
    private async Task DoWork()
    {
        _logger.Information("-------------------------------------------------------------------------------------------");
        _logger.Information("Task {TaskName} executed at: {CurrentTime}", nameof(ReportWorker), DateTime.Now);
        
        using var scope = _services.CreateScope();
        
        var intraDayReportService = 
            scope.ServiceProvider
                .GetRequiredService<IIntraDayReportService>();
            
        var exportReportsService = 
            scope.ServiceProvider
                .GetRequiredService<IExportReportsService>();
            
        var dateTimeProvider = 
            scope.ServiceProvider
                .GetRequiredService<IDateTimeProvider>();

        var handler = new IntraDayReportHandler(intraDayReportService, 
            exportReportsService, dateTimeProvider, logger);

        await handler.GenerateCsvReportAsync(_exportSettings.ReportPath);
        _logger.Information("-------------------------------------------------------------------------------------------");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Starting {WorkerName}.", nameof(ReportWorker));
        
        return Task.CompletedTask;
    }
}