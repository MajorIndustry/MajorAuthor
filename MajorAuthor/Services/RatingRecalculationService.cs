using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class RatingRecalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RatingRecalculationService> _logger;

        public RatingRecalculationService(
            IServiceProvider serviceProvider,
            ILogger<RatingRecalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var ratingManager = scope.ServiceProvider
                            .GetRequiredService<IRatingManagerService>();

                        _logger.LogInformation("Начинаю пересчет всех рейтингов...");
                        await ratingManager.RecalculateAllRatingsAsync();
                        _logger.LogInformation("Пересчет всех рейтингов завершен");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при пересчете рейтингов");
                }

                // Пересчитываем раз в сутки
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}