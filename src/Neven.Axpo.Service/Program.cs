using System;
using Axpo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neven.Axpo.Application.Services;
using Neven.Axpo.Domain.Entities;
using Neven.Axpo.Infrastructure.Services;
using Neven.Axpo.Service;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-failure-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHostedService<ReportWorker>();
    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    builder.Services.AddSerilog(logger);
    builder.Services.AddTransient<IExportReportsService, ExportReportsService>();
    builder.Services.AddTransient<IIntraDayReportService, IntraDayReportService>();
    builder.Services.AddTransient<IPowerService, PowerService>();
    builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
    var powerPositionsExportSettings = new PowerPositionsExportSettings();
    builder.Configuration.GetSection("PowerPositionsExportSettings").Bind(powerPositionsExportSettings);
    builder.Services.AddSingleton(powerPositionsExportSettings);
    var host = builder.Build();
    host.Run();
}
catch (Exception e)
{
    Log.Error(e, "Error Starting Service");
}
