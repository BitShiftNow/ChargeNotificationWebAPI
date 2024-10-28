using WebAPI.Services.Work.Items;

namespace WebAPI.Services.Work;

/// <summary>
/// A work item factory to create specific instances of <see cref="IWorkItem"/> objects.
/// </summary>
public interface IWorkItemFactory
{
    /// <summary>
    /// Creates a <see cref="AsyncFuncWorkItem"/> instance.
    /// </summary>
    IWorkItem FromAsyncFunc(Func<CancellationToken, Task> async_func);

    /// <summary>
    /// Creates a <see cref="CustomerChargeNotificationWorkItem"/> instance.
    /// </summary>
    IWorkItem CustomerChargeNotification(long customer_id, DateTime date);

    /// <summary>
    /// Creates a <see cref="AllChargeNotificationsWorkItem"/> instance.
    /// </summary>
    IWorkItem AllChargeNotifications(DateTime date);

    /// <summary>
    /// A work item that will set itself as completed once it has gone through the queue.
    /// </summary>
    IWorkItem CompletionWorkItem(IWorkItem item);
}

/// <summary>
/// The <see cref="IWorkItemFactory"/> implementation.
/// </summary>
public class WorkItemFactory : IWorkItemFactory {
    private long _next_id = 0;

    private readonly IServiceProvider services;

    public WorkItemFactory(IServiceProvider services) {
        this.services = services;
    }

    private long GetNextId() => Interlocked.Increment(ref _next_id);

    private WorkItemContext CreateContext() => new(GetNextId(), services);

    public IWorkItem FromAsyncFunc(Func<CancellationToken, Task> async_func)
        => new AsyncFuncWorkItem(CreateContext(), async_func);

    public IWorkItem CustomerChargeNotification(long customer_id, DateTime date)
        => new CustomerChargeNotificationWorkItem(CreateContext(), customer_id, date);

    public IWorkItem AllChargeNotifications(DateTime date)
        => new AllChargeNotificationsWorkItem2(CreateContext(), date);
        //=> new AllChargeNotificationsWorkItem(CreateContext(), date);

    public IWorkItem CompletionWorkItem(IWorkItem item)
        => new CompletionWorkItem(CreateContext(), item);
}

public static class WorkItemFactoryExtensions {
    public static IServiceCollection AddWorkItemFactory(this IServiceCollection collection) {
        collection.AddSingleton<IWorkItemFactory, WorkItemFactory>();
        return collection;
    }
}
