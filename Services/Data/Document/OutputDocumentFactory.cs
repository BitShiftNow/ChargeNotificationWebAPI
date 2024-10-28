using QuestPDF.Infrastructure;
using WebAPI.Models;
using WebAPI.Services.Data.Document.Schema;

namespace WebAPI.Services.Data.Document;

/// <summary>
/// The output document factory that creates QuestPDF documents based on a template.
/// </summary>
public interface IOutputDocumentFactory {
    IDocument ChargeNotificationDocument(ChargeDocumentTemplate template, ChargeNotification data);
}

public class OutputDocumentFactory : IOutputDocumentFactory {
    public IDocument ChargeNotificationDocument(ChargeDocumentTemplate template, ChargeNotification data) => new ChargeNotificationDocument(template, data);
}

public static class OutputDocumentFactoryExtensions {
    public static IServiceCollection AddDocumentFactory(this IServiceCollection collection) {
        collection.AddSingleton<IOutputDocumentFactory, OutputDocumentFactory>();
        return collection;
    }
}
