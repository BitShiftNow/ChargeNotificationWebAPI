namespace WebAPI.Models;

/// <summary>
/// The customer database entry.
/// This type is directly modelled to the database table and used by the <see cref="CustomerDbContext"/>.
/// </summary>
public class Customer {
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime RegisterDate { get; set; }
    public virtual ICollection<GameCharge> GameCharges { get; } = new List<GameCharge>();
}

/// <summary>
/// The customer DTO used on the APIs.
/// To translate a Customer to a CustomerDTO you can use the <see cref="Services.Data.Processors.CustomerDTOProcessor"/>
/// </summary>
public record CustomerDTO {
    public long Number { get; init; }
    public string Name { get; init; } = null!;
    public DateTime RegisterDate { get; init; }
}
