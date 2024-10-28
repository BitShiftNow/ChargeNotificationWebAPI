
namespace WebAPI.Services.Work.Items;

public class CompletionWorkItem : WorkItemBase {
    private readonly IWorkItemTracker tracker;
    private readonly IWorkItem item;

    public CompletionWorkItem(WorkItemContext context, IWorkItem item)
        : base(context) {
        this.item = item;
        tracker = services.GetRequiredService<IWorkItemTracker>();
    }

    public override Task DoWorkAsync(CancellationToken cancellationToken = default) {
        tracker.AddCompletedItem(item);
        return Task.CompletedTask;
    }
}
