using NexusMiner.Core.Models;

namespace NexusMiner.Core.Services;

/// <summary>
/// Calculates an expected-value scenario. It is not a pool payout predictor.
/// </summary>
public sealed class ProfitabilityCalculator
{
    public const decimal AverageDaysPerMonth = 30.4375m;
    private const decimal HoursPerDay = 24m;
    private const decimal WattsPerKilowatt = 1000m;

    public ProfitabilityResult Calculate(ProfitabilityInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Validate(input);

        var uptimeRatio = input.UptimePercent / 100m;
        var expectedCoinsPerDay = input.NetworkHashrateHps == 0m
            ? 0m
            : (input.DeviceHashrateHps / input.NetworkHashrateHps)
              * input.BlockRewardCoins
              * input.BlocksPerDay;

        var expectedCoinsPerMonth = expectedCoinsPerDay * AverageDaysPerMonth * uptimeRatio;
        var grossUsd = expectedCoinsPerMonth * input.CoinPriceUsd;
        var grossUyu = grossUsd * input.UyuPerUsd;
        var poolFeeUyu = grossUyu * (input.PoolFeePercent / 100m);
        var energyKwh = (input.PowerWatts / WattsPerKilowatt)
                        * HoursPerDay
                        * AverageDaysPerMonth
                        * uptimeRatio;
        var electricityCostUyu = energyKwh * input.ElectricityPriceUyuPerKwh;
        var netUyu = grossUyu - poolFeeUyu - electricityCostUyu - input.OtherMonthlyCostsUyu;
        var breakEvenRate = energyKwh > 0m
            ? (grossUyu - poolFeeUyu - input.OtherMonthlyCostsUyu) / energyKwh
            : null;

        return new ProfitabilityResult
        {
            ExpectedCoinsPerDay = expectedCoinsPerDay,
            ExpectedCoinsPerMonth = expectedCoinsPerMonth,
            GrossRevenueUsdPerMonth = grossUsd,
            GrossRevenueUyuPerMonth = grossUyu,
            PoolFeeUyuPerMonth = poolFeeUyu,
            EnergyKwhPerMonth = energyKwh,
            ElectricityCostUyuPerMonth = electricityCostUyu,
            OtherCostsUyuPerMonth = input.OtherMonthlyCostsUyu,
            NetUyuPerMonth = netUyu,
            BreakEvenElectricityPriceUyuPerKwh = breakEvenRate
        };
    }

    private static void Validate(ProfitabilityInput input)
    {
        RequireNonNegative(input.DeviceHashrateHps, nameof(input.DeviceHashrateHps));
        RequireNonNegative(input.NetworkHashrateHps, nameof(input.NetworkHashrateHps));
        RequireNonNegative(input.BlockRewardCoins, nameof(input.BlockRewardCoins));
        RequireNonNegative(input.BlocksPerDay, nameof(input.BlocksPerDay));
        RequireNonNegative(input.CoinPriceUsd, nameof(input.CoinPriceUsd));
        RequirePositive(input.UyuPerUsd, nameof(input.UyuPerUsd));
        RequireNonNegative(input.PowerWatts, nameof(input.PowerWatts));
        RequireNonNegative(input.ElectricityPriceUyuPerKwh, nameof(input.ElectricityPriceUyuPerKwh));
        RequireNonNegative(input.OtherMonthlyCostsUyu, nameof(input.OtherMonthlyCostsUyu));
        RequireRange(input.UptimePercent, 0m, 100m, nameof(input.UptimePercent));
        RequireRange(input.PoolFeePercent, 0m, 100m, nameof(input.PoolFeePercent));

        if (input.DeviceHashrateHps > 0m && input.NetworkHashrateHps <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.NetworkHashrateHps),
                "El hashrate de red debe ser mayor que cero cuando el equipo tiene hashrate.");
        }
    }

    private static void RequireNonNegative(decimal value, string name)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(name, "El valor no puede ser negativo.");
    }

    private static void RequirePositive(decimal value, string name)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(name, "El valor debe ser mayor que cero.");
    }

    private static void RequireRange(decimal value, decimal minimum, decimal maximum, string name)
    {
        if (value < minimum || value > maximum)
            throw new ArgumentOutOfRangeException(name, $"El valor debe estar entre {minimum} y {maximum}.");
    }
}
