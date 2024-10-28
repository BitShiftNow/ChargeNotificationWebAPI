using WebAPI.Services.Data;
using WebAPI.Services.Data.Document;
using WebAPI.Services.Data.Document.Template;
using WebAPI.Services.Data.Processors;
using WebAPI.Services.Work;

namespace WebAPI.Services;

public static class ServiceExtensions {
    public static IServiceCollection AddWebAPIServices(this IServiceCollection collection) {
        // Add work item services
        collection.AddWorkItemFactory();
        collection.AddWorkItemQueue();
        collection.AddWorkItemHostedService();
        collection.AddWorkItemTracker();

        // Add Data providers
        collection.AddCustomerDataProvider();

        // Add QuestPDF related services
        collection.AddTemplateProvider();
        collection.AddDocumentFactory();

        // Add Data processors
        collection.AddDataProcessors();

        return collection;
    }
}
