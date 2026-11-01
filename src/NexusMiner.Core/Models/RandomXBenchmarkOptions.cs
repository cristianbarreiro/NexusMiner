namespace NexusMiner.Core.Models;

/// <summary>
/// User-selected settings for a local, offline RandomX benchmark.
/// </summary>
public sealed record RandomXBenchmarkOptions
{
    public string BenchmarkSize { get; init; } = "250K";
    public int ThreadCount { get; init; } = 1;

    public long RequestedHashes => NormalizeSize(BenchmarkSize) switch
    {
        "250K" => 250_000L,
        "500K" => 500_000L,
        "1M" => 1_000_000L,
        _ => 0L
    };

    public string NormalizedBenchmarkSize => NormalizeSize(BenchmarkSize);

    public void Validate(int availableLogicalProcessors)
    {
        if (availableLogicalProcessors < 1)
            throw new ArgumentOutOfRangeException(nameof(availableLogicalProcessors));

        if (ThreadCount < 1 || ThreadCount > availableLogicalProcessors)
            throw new ArgumentOutOfRangeException(nameof(ThreadCount),
                $"La cantidad de hilos debe estar entre 1 y {availableLogicalProcessors}.");

        if (RequestedHashes == 0L)
            throw new ArgumentOutOfRangeException(nameof(BenchmarkSize),
                "El tamaño debe ser 250K, 500K o 1M.");
    }

    private static string NormalizeSize(string? size) =>
        (size ?? string.Empty).Trim().ToUpperInvariant();
}
