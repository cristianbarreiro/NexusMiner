namespace NexusMiner.Core.Models;

/// <summary>
/// User-supplied assumptions for an expected mining profitability scenario.
/// All monetary inputs are explicit; the app does not fetch market data.
/// </summary>
public sealed record ProfitabilityInput
{
    public decimal DeviceHashrateHps { get; init; }
    public decimal NetworkHashrateHps { get; init; }
    public decimal BlockRewardCoins { get; init; }
    public decimal BlocksPerDay { get; init; }
    public decimal CoinPriceUsd { get; init; }
    public decimal UyuPerUsd { get; init; }
    public decimal PowerWatts { get; init; }
    public decimal ElectricityPriceUyuPerKwh { get; init; }
    public decimal UptimePercent { get; init; } = 100m;
    public decimal PoolFeePercent { get; init; }
    public decimal OtherMonthlyCostsUyu { get; init; }
}
