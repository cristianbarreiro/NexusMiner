using NexusMiner.Core.Models;
using NexusMiner.Core.Services;

namespace NexusMiner.Core.Tests;

public sealed class ProfitabilityCalculatorTests
{
    private readonly ProfitabilityCalculator _calculator = new();

    [Fact]
    public void Calculate_ComputesExpectedCoinsFromHashrateShare()
    {
        var input = BaseInput() with
        {
            DeviceHashrateHps = 2_000m,
            NetworkHashrateHps = 4_000_000_000m,
            BlockRewardCoins = 0.6m,
            BlocksPerDay = 720m,
            UptimePercent = 100m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0.000216m, result.ExpectedCoinsPerDay);
        Assert.Equal(0.000216m * ProfitabilityCalculator.AverageDaysPerMonth, result.ExpectedCoinsPerMonth);
    }

    [Fact]
    public void Calculate_AppliesUptimeToCoinsAndEnergy()
    {
        var full = _calculator.Calculate(BaseInput() with { UptimePercent = 100m });
        var half = _calculator.Calculate(BaseInput() with { UptimePercent = 50m });

        Assert.Equal(full.ExpectedCoinsPerMonth / 2m, half.ExpectedCoinsPerMonth);
        Assert.Equal(full.EnergyKwhPerMonth / 2m, half.EnergyKwhPerMonth);
    }

    [Fact]
    public void Calculate_ComputesRevenueFeesElectricityAndNet()
    {
        var input = BaseInput() with
        {
            DeviceHashrateHps = 1m,
            NetworkHashrateHps = 1m,
            BlockRewardCoins = 1m,
            BlocksPerDay = 1m,
            CoinPriceUsd = 10m,
            UyuPerUsd = 40m,
            PowerWatts = 100m,
            ElectricityPriceUyuPerKwh = 10m,
            UptimePercent = 100m,
            PoolFeePercent = 2m,
            OtherMonthlyCostsUyu = 5m
        };

        var result = _calculator.Calculate(input);
        var expectedGross = ProfitabilityCalculator.AverageDaysPerMonth * 10m * 40m;
        var expectedEnergy = 0.1m * 24m * ProfitabilityCalculator.AverageDaysPerMonth;
        var expectedEnergyCost = expectedEnergy * 10m;

        Assert.Equal(expectedGross, result.GrossRevenueUyuPerMonth);
        Assert.Equal(expectedGross * 0.02m, result.PoolFeeUyuPerMonth);
        Assert.Equal(expectedEnergy, result.EnergyKwhPerMonth);
        Assert.Equal(expectedEnergyCost, result.ElectricityCostUyuPerMonth);
        Assert.Equal(expectedGross - expectedGross * 0.02m - expectedEnergyCost - 5m, result.NetUyuPerMonth);
    }

    [Fact]
    public void Calculate_ReturnsZeroCoinsWhenDeviceHashrateIsZero()
    {
        var result = _calculator.Calculate(BaseInput() with { DeviceHashrateHps = 0m, NetworkHashrateHps = 0m });

        Assert.Equal(0m, result.ExpectedCoinsPerDay);
        Assert.Equal(0m, result.GrossRevenueUyuPerMonth);
    }

    [Fact]
    public void Calculate_RejectsDeviceHashrateWithoutNetworkHashrate()
    {
        var input = BaseInput() with { DeviceHashrateHps = 1_000m, NetworkHashrateHps = 0m };

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(input));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void Calculate_RejectsUptimeOutsideRange(double uptime)
    {
        var input = BaseInput() with { UptimePercent = (decimal)uptime };

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(input));
    }

    [Fact]
    public void Calculate_RejectsZeroExchangeRate()
    {
        var input = BaseInput() with { UyuPerUsd = 0m };

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(input));
    }

    [Fact]
    public void Calculate_ReturnsNullBreakEvenRateWhenThereIsNoEnergyUse()
    {
        var result = _calculator.Calculate(BaseInput() with { PowerWatts = 0m });

        Assert.Null(result.BreakEvenElectricityPriceUyuPerKwh);
    }

    private static ProfitabilityInput BaseInput() => new()
    {
        DeviceHashrateHps = 1_000m,
        NetworkHashrateHps = 1_000_000_000m,
        BlockRewardCoins = 0.5m,
        BlocksPerDay = 720m,
        CoinPriceUsd = 300m,
        UyuPerUsd = 40m,
        PowerWatts = 50m,
        ElectricityPriceUyuPerKwh = 10m,
        UptimePercent = 100m,
        PoolFeePercent = 1m,
        OtherMonthlyCostsUyu = 0m
    };
}
