namespace WebAPI.Services.Work.Items;

/// <summary>
/// A <see cref="IWorkItem"/> that is able to execute an async <see cref="Func{CancellationToken, Task}"/>.
/// This could be used as a quick wrapper to test new work items before commiting to a full blown new <see cref="IWorkItem"/> implementation.
/// </summary>
public class AsyncFuncWorkItem : WorkItemBase {
    private readonly Func<CancellationToken, Task> async_func;

    public AsyncFuncWorkItem(WorkItemContext context, Func<CancellationToken, Task> async_func)
        : base(context) {
        this.async_func = async_func;
    }

    public override Task DoWorkAsync(CancellationToken cancellationToken = default)
        => async_func(cancellationToken);
}
