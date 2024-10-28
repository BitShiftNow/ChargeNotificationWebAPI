namespace WebAPI.Models;

/// <summary>
/// A processed charge notification that contains the total cost as well as
/// a breakdown of the charges in rows.
/// </summary>
public record ChargeNotification {
    public long Number { get; init; }
    public string Name { get; init; } = null!;
    public long TotalCost { get; init; }
    public IReadOnlyList<ChargeNotificationRow> Charges { get; init; } = new List<ChargeNotificationRow>();
}

/// <summary>
/// A charge notification breakdown row.
/// </summary>
public record ChargeNotificationRow {
    public DateTime Date { get; init; }
    public string Name { get; init; } = null!;
    public long Cost { get; init; }
}
