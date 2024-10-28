using System.Collections.Concurrent;
using System.Diagnostics;

namespace WebAPI.Services.Work;

/// <summary>
/// Used to check if a particular work item is done and reteive the time spent in the queue until completion.
/// </summary>
public interface IWorkItemStatusChecker {
    bool IsWorkItemComplete(long id, out TimeSpan time);
}

/// <summary>
/// Used to keep track of a particular work item.
/// </summary>
public interface IWorkItemTracker : IWorkItemStatusChecker {
    void AddCompletedItem(IWorkItem item);
}

/// <summary>
/// Used to track in-progress items.
/// </summary>
public class WorkItemTracker : IWorkItemTracker {
    private readonly ConcurrentDictionary<long, TimeSpan> completed = new();

    public void AddCompletedItem(IWorkItem item) {
        completed.AddOrUpdate(item.Id,
            _ => Stopwatch.GetElapsedTime(item.CreationTime),
            (_, _) => Stopwatch.GetElapsedTime(item.CreationTime));
    }

    public bool IsWorkItemComplete(long id, out TimeSpan time) {
        return completed.TryGetValue(id, out time);
    }

    public void Clear() {
        completed.Clear();
    }
}

public static class WorkItemTrackerExtensions {
    public static IServiceCollection AddWorkItemTracker(this IServiceCollection collection) {
        collection.AddSingleton<IWorkItemTracker, WorkItemTracker>();
        collection.AddSingleton<IWorkItemStatusChecker>(x => x.GetRequiredService<IWorkItemTracker>());
        return collection;
    }
}
