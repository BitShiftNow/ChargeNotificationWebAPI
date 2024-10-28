using WebAPI.Models;

namespace WebAPI.Services.Data.Processors;

public class CustomerDTOProcessor : IDataProcessor<Customer?, CustomerDTO?> {
    public CustomerDTO? Process(Customer? input) {
        CustomerDTO? result = null;

        if (input is not null) {
            result = new CustomerDTO() {
                Name = input.Name,
                Number = input.Id,
                RegisterDate = input.RegisterDate,
            };
        }

        return result;
    }
}

public static class CustomerDTOProcessorExtensions {
    public static IServiceCollection AddCustomerProcessor(this IServiceCollection collection) {
        collection.AddSingleton<IDataProcessor<Customer?, CustomerDTO?>, CustomerDTOProcessor>();
        return collection;
    }
}
