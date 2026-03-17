using FlowCare.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Services
{
    public class SlotCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SlotCleanupService> _logger;

        public SlotCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<SlotCleanupService> logger)
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
                    await CleanupExpiredSlots();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Slot cleanup failed.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupExpiredSlots()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var config = await context.Configs
                .FirstOrDefaultAsync(c => c.Key == "RetentionDays");

            var days = config != null ? int.Parse(config.Value) : 30;
            var cutoff = DateTime.UtcNow.AddDays(-days);

            var expiredSlots = await context.Slots
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted && s.DeletedAt < cutoff)
                .ToListAsync();

            if (!expiredSlots.Any())
            {
                _logger.LogInformation("No expired slots to clean up.");
                return;
            }

            foreach (var slot in expiredSlots)
            {
                var appointments = await context.Appointments
                    .Where(a => a.SlotId == slot.Id)
                    .ToListAsync();

                foreach (var appt in appointments)
                    appt.SlotId = null;
            }

            await context.SaveChangesAsync();

            context.Slots.RemoveRange(expiredSlots);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Cleaned up {Count} expired slots.", expiredSlots.Count);
        }
    }
}