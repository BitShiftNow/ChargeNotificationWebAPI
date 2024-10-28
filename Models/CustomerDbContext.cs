using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models;

/// <summary>
/// The customer database context used to read entries from the database.
/// Instead of using the context directly use a <see cref="Services.Data.ICustomerDataProvider"/> instance instead.
/// </summary>
public class CustomerDbContext : DbContext {
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<GameCharge> GameCharges { get; set; } = null!;

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) {
    }
}

public static class CustomerDbContextExtensions {
    public static IServiceCollection AddCustomerDbContext(this IServiceCollection collection) {
        collection.AddDbContextFactory<CustomerDbContext>(options => options.UseSqlServer("name=ConnectionStrings:ChargeNotificationWebApiDbConnectionString"));
        return collection;
    }
}
