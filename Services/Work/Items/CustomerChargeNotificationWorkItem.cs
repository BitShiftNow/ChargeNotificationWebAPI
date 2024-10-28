using QuestPDF.Fluent;
using WebAPI.Models;
using WebAPI.Options;
using WebAPI.Services.Data;
using WebAPI.Services.Data.Document;
using WebAPI.Services.Data.Document.Template;
using WebAPI.Services.Data.Processors;

namespace WebAPI.Services.Work.Items;

/// <summary>
/// This <see cref="IWorkItem"/> creates a charge notification PDF for a single customer and for a specific date.
/// It does not report back any results and can not be polled for completion.
/// </summary>
public class CustomerChargeNotificationWorkItem : WorkItemBase {
    private readonly long customer_id;
    private readonly DateTime date;

    private readonly ICustomerDataProvider provider;
    private readonly IDataProcessor<Customer?, ChargeNotification?> processor;
    private readonly IOutputDocumentFactory factory;
    private readonly ITemplateProvider template;

    private readonly ChargeNotificationOptions options;

    public CustomerChargeNotificationWorkItem(WorkItemContext context, long customer_id, DateTime date) : base(context) {
        this.customer_id = customer_id;
        this.date = date;

        provider = services.GetRequiredService<ICustomerDataProvider>();
        processor = services.GetRequiredService<IDataProcessor<Customer?, ChargeNotification?>>();
        factory = services.GetRequiredService<IOutputDocumentFactory>();
        template = services.GetRequiredService<ITemplateProvider>();

        options = services.GetRequiredService<IConfiguration>()
                    .GetSection(nameof(ChargeNotificationOptions))
                    .Get<ChargeNotificationOptions>()!;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default) {
        var raw_data = await provider.GetCustomerChargeNotificationAsync(customer_id, date, cancellationToken);
        var processed_data = processor.Process(raw_data);
        if (processed_data is not null && processed_data.Charges.Count > 0) {
            var doc_template = template.ReadChargeDocumentTemplate();
            var document = factory.ChargeNotificationDocument(doc_template, processed_data);
            var date_string = processed_data.Charges[0].Date.ToString("yyyy-M-dd");

            if (!Directory.Exists(options.DocumentOutputDirectory)) {
                Directory.CreateDirectory(options.DocumentOutputDirectory);
            }
            var filename = Path.Combine(options.DocumentOutputDirectory, $"{processed_data.Number}.{date_string}.pdf");
            document.GeneratePdf(filename);
        }
    }
}
