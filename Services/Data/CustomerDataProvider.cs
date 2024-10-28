using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WebAPI.Models;

namespace WebAPI.Services.Data;

/// <summary>
/// This interface is able to fetch customer and charge data from the database.
/// </summary>
public interface ICustomerDataProvider {
    Task<Customer?> GetCustomerChargeNotificationAsync(long customer_id, DateTime date, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetAllChargeNotificationAsync(DateTime date, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Customer> GetAllCustomersAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetSingleCustomerAsync(long number, CancellationToken cancellationToken = default);
    Task<Customer> InsertNewCustomerAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> RemoveCustomerAsync(long number, CancellationToken cancellationToken = default);
    Task<GameCharge?> GetSingleGameChargeAsync(long number, CancellationToken cancellationToken = default);
    Task<GameCharge> InsertNewGameChargeAsync(long customer_id, long game_id, string game_name, DateTime charge_date, long cost, CancellationToken cancellationToken = default);
    Task<bool> RemoveGameChargeAsync(long number, CancellationToken cancellationToken = default);
    Task InsertRandomCustomersAsync(int count, CancellationToken cancellationToken = default);
    Task InsertRandomCustomerChargesAsync(int count, DateTime date, CancellationToken cancellationToken = default);
}

/// <summary>
/// The <see cref="ICustomerDataProvider"/> implementation.
/// 
/// This is essentially the main link to the database and where all queries are performed.
/// This means I do not need to pass the <see cref="CustomerDbContext"/> around directly,
/// but it also means I need to provide a lot of specialised methods to fetch specific things.
/// 
/// After using it, and now finalizing it, I am not too happy with this approach
/// and probably would just use the CustomerDbContext directly. Or rather a <see cref="IDbContextFactory{CustomerDbContext}"/>.
/// </summary>
public class CustomerDataProvider : ICustomerDataProvider {
    private readonly IDbContextFactory<CustomerDbContext> db_context_factory;

    public CustomerDataProvider(IDbContextFactory<CustomerDbContext> db_context_factory) {
        this.db_context_factory = db_context_factory;
    }

    /// <summary>
    /// Gets a specific customer with all game charges for a specific date.
    /// </summary>
    /// <param name="customer_id">The customer number</param>
    /// <param name="date">The date of the game charges to retreive. The time portion is ignored.</param>
    public async Task<Customer?> GetCustomerChargeNotificationAsync(long customer_id, DateTime date, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var customer = await db.Customers.Where(x => x.Id == customer_id)
            .Include(x => x.GameCharges.Where(c => c.ChargeDate.Date == date.Date))
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }

    public async Task<List<Customer>> GetAllChargeNotificationAsync(DateTime date, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var result = await db.Customers
            .Include(x => x.GameCharges.Where(c => c.ChargeDate.Date == date.Date))
            .ToListAsync(cancellationToken);
        if (result is null) {
            result = new List<Customer>();
        }
        return result;
    }

    public async IAsyncEnumerable<Customer> GetAllCustomersAsync([EnumeratorCancellation]CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        await foreach(var customer in db.Customers.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
            yield return customer;
        }
    }

    public async Task<Customer?> GetSingleCustomerAsync(long number, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var customer = await db.Customers.FindAsync(number, cancellationToken);
        return customer;
    }

    public async Task<Customer> InsertNewCustomerAsync(string name, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var new_customer = new Customer() { Name = name, RegisterDate = DateTime.UtcNow };

        db.Customers.Add(new_customer);
        await db.SaveChangesAsync(cancellationToken);

        return new_customer;
    }

    public async Task<bool> RemoveCustomerAsync(long number, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var customer = await db.Customers.FindAsync(number, cancellationToken);
        var result = customer is not null;

        if (result) {
            db.Customers.Remove(customer!);
            await db.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<GameCharge?> GetSingleGameChargeAsync(long number, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var charge = await db.GameCharges.FindAsync(number, cancellationToken);
        return charge;
    }

    public async Task<GameCharge> InsertNewGameChargeAsync(long customer_id, long game_id, string game_name, DateTime charge_date, long cost, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var new_charge = new GameCharge() {
            CustomerId = customer_id,
            ChargeDate = charge_date,
            GameId = game_id,
            GameName = game_name,
            Cost = cost,
        };

        db.GameCharges.Add(new_charge);
        await db.SaveChangesAsync(cancellationToken);

        return new_charge;
    }

    public async Task<bool> RemoveGameChargeAsync(long number, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        var charge = await db.GameCharges.FindAsync(number, cancellationToken);
        var result = charge is not null;

        if (result) {
            db.GameCharges.Remove(charge!);
            await db.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task InsertRandomCustomersAsync(int count, CancellationToken cancellationToken = default) {
        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        for (var i = 0; i < count; i++) {
            var new_customer = new Customer() { Name = $"Customer {i}", RegisterDate = DateTime.UtcNow };
            db.Customers.Add(new_customer);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertRandomCustomerChargesAsync(int count, DateTime date, CancellationToken cancellationToken = default) {
        // Some example games. This would usually be in the database but for testing purposes I have hardcoded them in there
        (long id, string name)[] games = {
            (0L, "The Talos Principle"),
            (1L, "The Talos Principle 2"),
            (2L, "The Witness"),
            (3L, "Braid"),
            (4L, "Trinity"),
            (5L, "Animal Well"),
            (6L, "Slipways"),
            (7L, "Stardew Valley"),
            (8L, "SHENZHEN I/O"),
            (9L, "Factorio"),
        };

        using var db = await db_context_factory.CreateDbContextAsync(cancellationToken);

        await foreach (var customer in db.Customers.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
            // Insert a random amount between 1 and count
            for (var i = 0; i < Random.Shared.Next(count); i++) {
                var (id, name) = games[Random.Shared.Next(0, games.Length)];
                var new_charge = new GameCharge() {
                    CustomerId = customer.Id,
                    GameId = id,
                    GameName = name,
                    Cost = Random.Shared.Next(1, 999),
                    ChargeDate = date,
                };
                customer.GameCharges.Add(new_charge);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

public static class CustomerDataProviderExtensions {
    /// <summary>
    /// Adds a transient <see cref="ICustomerDataProvider"/> instance.
    /// </summary>
    public static IServiceCollection AddCustomerDataProvider(this IServiceCollection collection) {
        collection.AddTransient<ICustomerDataProvider, CustomerDataProvider>();
        return collection;
    }
}
