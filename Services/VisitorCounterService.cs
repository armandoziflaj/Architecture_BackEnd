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

public class VisitorCountUpdateService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var counterService = scope.ServiceProvider.GetRequiredService<VisitorCounterService>();
            var initialCount = dbContext.VisitorCounters.FirstOrDefault()?.Count ?? 0;
            counterService.SetInitialCount(initialCount);
        }

        _timer = new Timer(UpdateVisitorCountInDatabase, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }
    
    private void UpdateVisitorCountInDatabase(object? state)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var counterService = scope.ServiceProvider.GetRequiredService<VisitorCounterService>();

        var totalCount = counterService.GetCount();

        var counter = dbContext.VisitorCounters.FirstOrDefault();
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
