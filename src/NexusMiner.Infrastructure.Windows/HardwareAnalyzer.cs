using System.Management;
using NexusMiner.Core.Models;

namespace NexusMiner.Infrastructure.Windows;

/// <summary>
/// Reads local Windows hardware information through WMI. No network calls are made.
/// </summary>
public sealed class HardwareAnalyzer
{
    public Task<HardwareSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => GetSnapshot(cancellationToken), cancellationToken);
    }

    private static HardwareSnapshot GetSnapshot(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var warnings = new List<string>();
        var processorNames = new List<string>();
        var physicalCores = 0;
        var logicalProcessors = 0;
        var operatingSystem = Environment.OSVersion.ToString();
        var totalMemoryGb = 0m;
        var gpus = new List<GpuInfo>();

        TryWmi("Win32_Processor", "SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor", warnings, rows =>
        {
            foreach (var row in rows)
            {
                using (row)
                {
                    var name = ReadString(row, "Name");
                    if (!string.IsNullOrWhiteSpace(name)) processorNames.Add(name);
                    physicalCores += ReadInt(row, "NumberOfCores");
                    logicalProcessors += ReadInt(row, "NumberOfLogicalProcessors");
                }
            }
        });

        TryWmi("Win32_ComputerSystem", "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", warnings, rows =>
        {
            var first = rows.FirstOrDefault();
            if (first is not null)
            {
                using (first)
                {
                    var bytes = ReadDecimal(first, "TotalPhysicalMemory");
                    totalMemoryGb = bytes / 1_073_741_824m;
                }
            }
            foreach (var extra in rows.Skip(1)) extra.Dispose();
        });

        TryWmi("Win32_OperatingSystem", "SELECT Caption FROM Win32_OperatingSystem", warnings, rows =>
        {
            var first = rows.FirstOrDefault();
            if (first is not null)
            {
                using (first)
                {
                    var caption = ReadString(first, "Caption");
                    if (!string.IsNullOrWhiteSpace(caption)) operatingSystem = caption;
                }
            }
            foreach (var extra in rows.Skip(1)) extra.Dispose();
        });

        TryWmi("Win32_VideoController", "SELECT Name, AdapterRAM FROM Win32_VideoController", warnings, rows =>
        {
            foreach (var row in rows)
            {
                using (row)
                {
                    var name = ReadString(row, "Name");
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    var bytes = ReadDecimal(row, "AdapterRAM");
                    decimal? memoryGb = bytes > 0m ? bytes / 1_073_741_824m : null;
                    gpus.Add(new GpuInfo(name, memoryGb));
                }
            }
        });

        cancellationToken.ThrowIfCancellationRequested();

        return new HardwareSnapshot
        {
            OperatingSystem = operatingSystem,
            ProcessorName = processorNames.Count == 0 ? "No detectado" : string.Join("; ", processorNames.Distinct()),
            PhysicalCores = physicalCores,
            LogicalProcessors = logicalProcessors,
            TotalMemoryGb = totalMemoryGb,
            Gpus = gpus,
            CapturedAt = DateTimeOffset.Now,
            Warnings = warnings
        };
    }

    private static void TryWmi(
        string component,
        string query,
        ICollection<string> warnings,
        Action<IReadOnlyList<ManagementBaseObject>> consume)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            consume(collection.Cast<ManagementBaseObject>().ToArray());
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            warnings.Add($"No se pudo consultar {component}: {ex.Message}");
        }
    }

    private static string ReadString(ManagementBaseObject row, string propertyName)
    {
        try { return Convert.ToString(row[propertyName])?.Trim() ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static int ReadInt(ManagementBaseObject row, string propertyName)
    {
        try { return Convert.ToInt32(row[propertyName]); }
        catch { return 0; }
    }

    private static decimal ReadDecimal(ManagementBaseObject row, string propertyName)
    {
        try { return Convert.ToDecimal(row[propertyName]); }
        catch { return 0m; }
    }
}
