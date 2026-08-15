using Sulozeqi_BackEnd.Models;

namespace Sulozeqi_BackEnd.Services;

public class VisitorCounterService
{
    private long _count;

    public void Increment()
    {
        Interlocked.Increment(ref _count);
    }

    public long GetCount()
    {
        return Interlocked.Read(ref _count);
    }

    public void SetInitialCount(long count)
    {
        Interlocked.Exchange(ref _count, count);
    }
}

public class VisitorCountUpdateService(IServiceProvider serviceProvider,IConfiguration configuration, ILogger<VisitorCountUpdateService> logger) : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly long _updateFrequency = configuration.GetValue("VisitorCounterSettings:UpdateFrequencyMinutes", 1);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var counterService = scope.ServiceProvider.GetRequiredService<VisitorCounterService>();
            var initialCount = dbContext.VisitorCounters.SingleOrDefault()?.Count ?? 0;
            logger.LogInformation("VisitorCountUpdateService started. Initial count loaded: {InitialCount}", initialCount); 
            counterService.SetInitialCount(initialCount);
        }

        _timer = new Timer(UpdateVisitorCountInDatabase, null, TimeSpan.FromMinutes(_updateFrequency), TimeSpan.FromMinutes(_updateFrequency));
        return Task.CompletedTask;
    }
    
    private void UpdateVisitorCountInDatabase(object? state)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var counterService = scope.ServiceProvider.GetRequiredService<VisitorCounterService>();

        var totalCount = counterService.GetCount();

        var counter = dbContext.VisitorCounters.SingleOrDefault();
        if (counter == null)
        {
            counter = new VisitorCounter { Count = totalCount, LastUpdated = DateTime.UtcNow };
            dbContext.VisitorCounters.Add(counter);
        }
        else
        {
            counter.Count = totalCount;
            counter.LastUpdated = DateTime.UtcNow;
        }
        dbContext.SaveChanges();
        logger.LogInformation("Successfully saved total visitor count to database: {TotalCount}", totalCount);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        UpdateVisitorCountInDatabase(null);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
