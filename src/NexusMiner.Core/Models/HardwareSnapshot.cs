namespace NexusMiner.Core.Models;

public sealed record GpuInfo(string Name, decimal? AdapterMemoryGb);

public sealed record HardwareSnapshot
{
    public required string OperatingSystem { get; init; }
    public required string ProcessorName { get; init; }
    public required int PhysicalCores { get; init; }
    public required int LogicalProcessors { get; init; }
    public required decimal TotalMemoryGb { get; init; }
    public required IReadOnlyList<GpuInfo> Gpus { get; init; }
    public required DateTimeOffset CapturedAt { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
}
