namespace NexusMiner.Core.Models;

public sealed record ProfitabilityResult
{
    public required decimal ExpectedCoinsPerDay { get; init; }
    public required decimal ExpectedCoinsPerMonth { get; init; }
    public required decimal GrossRevenueUsdPerMonth { get; init; }
    public required decimal GrossRevenueUyuPerMonth { get; init; }
    public required decimal PoolFeeUyuPerMonth { get; init; }
    public required decimal EnergyKwhPerMonth { get; init; }
    public required decimal ElectricityCostUyuPerMonth { get; init; }
    public required decimal OtherCostsUyuPerMonth { get; init; }
    public required decimal NetUyuPerMonth { get; init; }
    public decimal? BreakEvenElectricityPriceUyuPerKwh { get; init; }
    public bool IsProfitable => NetUyuPerMonth > 0m;
}
