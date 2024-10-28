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

public record CustomerDTO {
    public long Number { get; init; }
    public string Name { get; init; } = null!;
    public DateTime RegisterDate { get; init; }
}
