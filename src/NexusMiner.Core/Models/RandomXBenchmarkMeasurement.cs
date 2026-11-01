namespace NexusMiner.Core.Models;

/// <summary>Parsed result from an XMRig offline RandomX benchmark.</summary>
public sealed record RandomXBenchmarkMeasurement
{
    public required decimal HashrateHps { get; init; }
    public required decimal ElapsedSeconds { get; init; }
    public required string Checksum { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
}
