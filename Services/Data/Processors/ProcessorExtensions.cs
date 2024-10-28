namespace WebAPI.Services.Data.Processors;

public static class ProcessorExtensions {
    public static IServiceCollection AddDataProcessors(this IServiceCollection collection) {
        collection.AddChargeNotificationProcessor();
        collection.AddCustomerProcessor();
        collection.AddGameChargeProcessor();
        return collection;
    }
}
