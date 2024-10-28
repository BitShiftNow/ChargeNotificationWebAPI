namespace WebAPI.Services.Work;

/// <summary>
/// A hosted work item background service.
/// This service creates a new <see cref="IWorkItemQueue"/> instance via a <see cref="IWorkItemQueueFactory"/>.
/// Once the service is started it will fetch and execute single <see cref="IWorkItem"/> objects in order
/// they have been queued.
/// </summary>
public class WorkItemHostedService : BackgroundService {
    private readonly IWorkItemQueue queue;
    private readonly ILogger<WorkItemHostedService> log;

    public WorkItemHostedService(ILogger<WorkItemHostedService> log, IWorkItemQueue queue) {
        this.log = log;
        this.queue = queue;
    }

    public override Task StartAsync(CancellationToken cancellationToken) {
        log.LogInformation("Starting work item service");

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        log.LogInformation("Stopping work item service");

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default) {
        try {
            log.LogInformation("Executing work item queue items");

            await foreach (var item in queue.ReadAllWorkItemsAsync(cancellationToken)) {
                try {
                    log.LogDebug("Executing work item {0}", item.Id);

                    await item.DoWorkAsync(cancellationToken);

                } catch (Exception ex) when (ex is not OperationCanceledException) {
                    log.LogError(ex, "Failed to execute work item {0}", item.Id);
                }
            }
        } catch (OperationCanceledException) {
            log.LogInformation("Work item execution has been cancelled");
            queue.Close();
        }
    }
}

public static class WorkItemHostedServiceExtensions {
    public static IServiceCollection AddWorkItemHostedService(this IServiceCollection collection) {
        collection.AddHostedService<WorkItemHostedService>();
        return collection;
    }
}

