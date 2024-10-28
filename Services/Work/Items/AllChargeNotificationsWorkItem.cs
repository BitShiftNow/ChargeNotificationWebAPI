using QuestPDF.Fluent;
using WebAPI.Models;
using WebAPI.Options;
using WebAPI.Services.Data;
using WebAPI.Services.Data.Document;
using WebAPI.Services.Data.Document.Template;
using WebAPI.Services.Data.Processors;

namespace WebAPI.Services.Work.Items;

/// <summary>
/// My first attempt at creating all charge notifications.
/// It was queuing every single document generation as a separate new work item.
/// Took about 3 minutes for ~30k work items.
/// </summary>
public class AllChargeNotificationsWorkItem : WorkItemBase {
    private readonly DateTime date;

    private readonly ICustomerDataProvider provider;
    private readonly IWorkItemFactory factory;
    private readonly IWorkItemQueueWriter queue;

    public AllChargeNotificationsWorkItem(WorkItemContext context, DateTime date) : base(context) {
        this.date = date;
        
        provider = services.GetRequiredService<ICustomerDataProvider>();
        factory = services.GetRequiredService<IWorkItemFactory>();
        queue = services.GetRequiredService<IWorkItemQueueWriter>();
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default) {
        await foreach(var customer in provider.GetAllCustomersAsync(cancellationToken)) {
            var work_item = factory.CustomerChargeNotification(customer.Id, date);
            await queue.QueueWorkItemAsync(work_item);
        }

        var completion_item = factory.CompletionWorkItem(this);
        await queue.QueueWorkItemAsync(completion_item);
    }
}

/// <summary>
/// My second attempt at creating all charge notifications.
/// This time it is all done in a single work item.
/// Took about 42 seconds for ~30k work items.
/// </summary>
public class AllChargeNotificationsWorkItem2 : WorkItemBase {
    private readonly DateTime date;

    private readonly ICustomerDataProvider provider;
    private readonly IDataProcessor<Customer?, ChargeNotification?> processor;
    private readonly IOutputDocumentFactory document_factory;
    private readonly IWorkItemTracker tracker;
    private readonly ITemplateProvider template;

    private readonly ChargeNotificationOptions options;

    public AllChargeNotificationsWorkItem2(WorkItemContext context, DateTime date) : base(context) {
        this.date = date;

        provider = services.GetRequiredService<ICustomerDataProvider>();
        processor = services.GetRequiredService<IDataProcessor<Customer?, ChargeNotification?>>();
        document_factory = services.GetRequiredService<IOutputDocumentFactory>();
        tracker = services.GetRequiredService<IWorkItemTracker>();
        template = services.GetRequiredService<ITemplateProvider>();

        options = services.GetRequiredService<IConfiguration>()
                    .GetSection(nameof(ChargeNotificationOptions))
                    .Get<ChargeNotificationOptions>()!;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default) {
        var customers = await provider.GetAllChargeNotificationAsync(date, cancellationToken);

        var notifications = customers.Select(x => processor.Process(x))
            .Where(x => x is { Charges.Count: > 0 });

        if (!Directory.Exists(options.DocumentOutputDirectory)) {
            Directory.CreateDirectory(options.DocumentOutputDirectory);
        }

        var doc_template = template.ReadChargeDocumentTemplate();
        Parallel.ForEach(notifications, (notification) => {
            var document = document_factory.ChargeNotificationDocument(doc_template, notification!);
            var date_string = notification!.Charges[0].Date.ToString("yyyy-M-dd");

            var filename = Path.Combine(options.DocumentOutputDirectory, $"{notification.Number}.{date_string}.pdf");
            document.GeneratePdf(filename);
        });

        tracker.AddCompletedItem(this);
    }
}
