using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace WebAPI.Services.Work;

/// <summary>
/// Most services need to be able to queue a new <see cref="IWorkItem"/>.
/// This interface provides that functionality without expising all the other <see cref="IWorkItemQueue"/> methods.
/// </summary>
public interface IWorkItemQueueWriter {
    ValueTask<bool> QueueWorkItemAsync(IWorkItem item, CancellationToken cancellationToken = default);
}

/// <summary>
/// A queue of <see cref="IWorkItem"/> to be processed by the <see cref="WorkItemHostedService"/>.
/// </summary>
public interface IWorkItemQueue : IWorkItemQueueWriter {
    ValueTask<bool> WaitForWorkItemAsync(CancellationToken cancellationToken = default);
    bool TryReadWorkItem(out IWorkItem item);
    void Close();
}

/// <summary>
/// The <see cref="IWorkItemQueue"/> implementation.
/// </summary>
public class WorkItemQueue : IWorkItemQueue {
    private readonly Channel<IWorkItem> channel = Channel.CreateUnbounded<IWorkItem>(new UnboundedChannelOptions() {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
    });

    public void Close() => channel.Writer.TryComplete();

    /// <summary>
    /// Attempts to queue a new <see cref="IWorkItem"/> as fast as possible.
    /// If the item can not be queued an asynchronous slow path will be used instead.
    /// </summary>
    /// <param name="item">The work item to put into the queue</param>
    /// <param name="cancellationToken">The cancellation token. This will only be used in the slow path of the queue.</param>
    /// <returns>False is the queue is closed.</returns>
    public ValueTask<bool> QueueWorkItemAsync(IWorkItem item, CancellationToken cancellationToken = default) {
        var is_success = channel.Writer.TryWrite(item);
        return is_success ? ValueTask.FromResult(true) : QueueWorkItemSlowAsync(channel.Writer, item, cancellationToken);

        static async ValueTask<bool> QueueWorkItemSlowAsync(ChannelWriter<IWorkItem> writer, IWorkItem item, CancellationToken cancellationToken) {
            var is_success = writer.TryWrite(item);
            if (is_success is false) {
                try {
                    cancellationToken.ThrowIfCancellationRequested();

                    await writer.WriteAsync(item, cancellationToken);
                    is_success = true;
                } catch (OperationCanceledException) {
                    is_success = false;
                }
            }
            return is_success;
        }
    }

    public bool TryReadWorkItem(out IWorkItem item)
        => channel.Reader.TryRead(out item!);

    public ValueTask<bool> WaitForWorkItemAsync(CancellationToken cancellationToken = default)
        => channel.Reader.WaitToReadAsync(cancellationToken);
}

public static class WorkItemQueueExtensions {
    /// <summary>
    /// Depleting a <see cref="IWorkItemQueue"/> as an <see cref="IAsyncEnumerable{IWorkItem}"/>
    /// </summary>
    public static async IAsyncEnumerable<IWorkItem> ReadAllWorkItemsAsync(this IWorkItemQueue queue, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        while (await queue.WaitForWorkItemAsync(cancellationToken)) {
            while (queue.TryReadWorkItem(out var item)) {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Adds a <see cref="IWorkItemQueue"/> as a singleton.
    /// </summary>
    public static IServiceCollection AddWorkItemQueue(this IServiceCollection collection) {
        collection.AddSingleton<IWorkItemQueue, WorkItemQueue>();
        collection.AddSingleton<IWorkItemQueueWriter>(x => x.GetRequiredService<IWorkItemQueue>());
        return collection;
    }
}
