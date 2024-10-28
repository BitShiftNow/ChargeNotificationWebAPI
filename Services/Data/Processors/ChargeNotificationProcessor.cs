using WebAPI.Models;

namespace WebAPI.Services.Data.Processors;

public class ChargeNotificationProcessor : IDataProcessor<Customer?, ChargeNotification?> {
    public ChargeNotification? Process(Customer? input) {
        ChargeNotification? result = null;
        if (input is not null) {
            var charges = new List<ChargeNotificationRow>(input.GameCharges.Count);

            foreach (var group in input.GameCharges.GroupBy(x => x.GameId)) {
                var first = group.First();
                var charge = new ChargeNotificationRow() {
                    Date = first.ChargeDate,
                    Name = first.GameName,
                    Cost = group.Sum(x => x.Cost),
                };
                charges.Add(charge);
            }

            result = new ChargeNotification() {
                Name = input.Name,
                Number = input.Id,
                Charges = charges,
                TotalCost = charges.Sum(x => x.Cost),
            };
        }
        return result;
    }
}

public static class ChargeNotificationProcessorExtensions {
    public static IServiceCollection AddChargeNotificationProcessor(this IServiceCollection collection) {
        collection.AddSingleton<IDataProcessor<Customer?, ChargeNotification?>, ChargeNotificationProcessor>();
        return collection;
    }
}
