using GymSystem.Application.Memberships;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymSystem.Infrastructure.BackgroundJobs;

public class MembershipExpiryWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MembershipExpiryWorker> _logger;

    public MembershipExpiryWorker(
        IServiceScopeFactory scopeFactory, ILogger<MembershipExpiryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // normal during shutdown, not an error
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMembershipExpiryService>();

            var expired = await service.ExpireDueMembershipsAsync(ct);

            _logger.LogInformation(
                "Membership expiry run finished: {Count} membership(s) expired.", expired);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Membership expiry run failed. It will be retried at the next interval.");
        }
    }
}