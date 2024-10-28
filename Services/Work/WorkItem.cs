using System.Diagnostics;

namespace WebAPI.Services.Work;

/// <summary>
/// A single work item to be executed by the <see cref="WorkItemHostedService"/>
/// </summary>
public interface IWorkItem {
    long Id { get; }
    long CreationTime { get; }
    Task DoWorkAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The work item context to provide data to the <see cref="WorkItemBase"/>.
/// This will reduce constructor arguments for classes that implement <see cref="WorkItemBase"/>.
/// </summary>
public readonly struct WorkItemContext {
    public readonly long id;
    public readonly IServiceProvider services;

    public WorkItemContext(long id, IServiceProvider services) {
        this.id = id;
        this.services = services;
    }
}

/// <summary>
/// The base class for all work item implementations.
/// This does conveniently provide the WorkItem id and a service provider
/// </summary>
public abstract class WorkItemBase : IWorkItem {
    public long Id { get; init; }
    public long CreationTime { get; init; } = Stopwatch.GetTimestamp();

    protected readonly IServiceProvider services;

    protected WorkItemBase(WorkItemContext context) {
        Id = context.id;
        services = context.services;
    }

    public abstract Task DoWorkAsync(CancellationToken cancellationToken = default);
}
