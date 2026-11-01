using NexusMiner.Core.Models;
using NexusMiner.Core.Services;
using Xunit;

namespace NexusMiner.Core.Tests;

public sealed class RandomXBenchmarkOutputParserTests
{
    [Fact]
    public void TryParse_ParsesHpsAndChecksum()
    {
        const string output = "bench benchmark finished in 200.000 seconds (1250.0 H/s) hash sum = 898B6E0431C28A6B";
        var parsed = RandomXBenchmarkOutputParser.TryParse(output, out var result);
        Assert.True(parsed);
        Assert.NotNull(result);
        Assert.Equal(1_250m, result.HashrateHps);
        Assert.Equal(200m, result.ElapsedSeconds);
        Assert.Equal("898B6E0431C28A6B", result.Checksum);
    }

    [Theory]
    [InlineData("1.25 kH/s", 1250)]
    [InlineData("0.00125 MH/s", 1250)]
    public void TryParse_NormalizesHashrateUnits(string rate, decimal expected)
    {
        var output = $"bench benchmark finished in 10.0 seconds ({rate}) hash sum = 898B6E0431C28A6B";
        Assert.True(RandomXBenchmarkOutputParser.TryParse(output, out var result));
        Assert.NotNull(result);
        Assert.Equal(expected, result.HashrateHps);
    }

    [Fact]
    public void TryParse_IgnoresAnsiColorSequences()
    {
        const string output = "\u001b[32mbench benchmark finished in 10.0 seconds (500.0 H/s) hash sum = 898B6E0431C28A6B\u001b[0m";
        Assert.True(RandomXBenchmarkOutputParser.TryParse(output, out var result));
        Assert.NotNull(result);
        Assert.Equal(500m, result.HashrateHps);
    }

    [Theory]
    [InlineData("")]
    [InlineData("benchmark started but did not finish")]
    [InlineData("benchmark finished in 0 seconds (0 H/s) hash sum = 898B6E0431C28A6B")]
    [InlineData("benchmark finished in 10 seconds (500 H/s) hash sum = not-a-checksum")]
    public void TryParse_RejectsInvalidOutput(string output)
    {
        Assert.False(RandomXBenchmarkOutputParser.TryParse(output, out var result));
        Assert.Null(result);
    }
}

public sealed class RandomXBenchmarkOptionsTests
{
    [Theory]
    [InlineData("250K", 250_000L)]
    [InlineData("500K", 500_000L)]
    [InlineData("1M", 1_000_000L)]
    public void Validate_AcceptsSupportedOfflineSizes(string size, long hashes)
    {
        var options = new RandomXBenchmarkOptions { BenchmarkSize = size, ThreadCount = 3 };
        options.Validate(8);
        Assert.Equal(hashes, options.RequestedHashes);
    }

    [Fact]
    public void Validate_RejectsUnsupportedSize()
    {
        var options = new RandomXBenchmarkOptions { BenchmarkSize = "10M", ThreadCount = 1 };
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate(8));
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(9, 8)]
    [InlineData(1, 0)]
    public void Validate_RejectsInvalidThreadCount(int threads, int available)
    {
        var options = new RandomXBenchmarkOptions { BenchmarkSize = "250K", ThreadCount = threads };
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate(available));
    }
}
