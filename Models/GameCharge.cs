using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models;

/// <summary>
/// The game charge database entry.
/// This type is directly modelled to the database table and used by the <see cref="CustomerDbContext"/>.
/// </summary>
[Index(nameof(CustomerId), nameof(ChargeDate), nameof(GameId), IsDescending = [false, true, false])]
public class GameCharge {
    public long Id { get; set; }
    public long Cost { get; set; }
    public DateTime ChargeDate { get; set; }
    public long GameId { get; set; } // Pretending we have a table of games. GameName should come from that ideally but in this excercise it is included in the charges.
    public string GameName { get; set; } = null!;
    public long CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
}

/// <summary>
/// The game charge DTO used by the APIs.
/// To translate a GameCharge to a GameChargeDTO you can use the <see cref="Services.Data.Processors.GameChargeDTOProcessor"/>
/// </summary>
public class GameChargeDTO {
    public long Number { get; init; }
    public long Cost { get; init; }
    public DateTime ChargeDate { get; init; }
    public long Game { get; init; }
    public string GameName { get; init; } = null!;
    public long Customer { get; init; }
}
