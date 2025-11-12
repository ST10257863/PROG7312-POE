using Municipality_Application.Interfaces.Service;

namespace Municipality_Application.Services
{
    public class ReportPreloadBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportPreloadBackgroundService> _logger;

        public ReportPreloadBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReportPreloadBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Background preload: loading latest reports into memory...");
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                        await reportService.ListReportsAsync(forceRefresh: true);
                    }
                    _logger.LogInformation("Background preload: reports loaded and cached.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background preload of reports.");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}