namespace Ceres9.Core.Domain;

public sealed record WorkOrder(
    long OrderId,
    int ShipId,
    string Company,
    string CargoType,
    int Crates,
    int Credits,
    DateTimeOffset CreatedAtUtc);

