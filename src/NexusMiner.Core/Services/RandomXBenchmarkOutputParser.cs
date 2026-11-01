using System.Globalization;
using System.Text.RegularExpressions;
using NexusMiner.Core.Models;

namespace NexusMiner.Core.Services;

/// <summary>Parses XMRig's final offline benchmark summary line.</summary>
public static class RandomXBenchmarkOutputParser
{
    private static readonly Regex AnsiEscapePattern = new(
        @"\x1B\[[0-?]*[ -/]*[@-~]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CompletionPattern = new(
        @"benchmark\s+finished\s+in\s+(?<seconds>\d+(?:\.\d+)?)\s+seconds\s+\((?<rate>\d+(?:\.\d+)?)\s+(?<unit>MH/s|kH/s|H/s)\)\s+hash\s+sum\s*=\s*(?<checksum>[A-Fa-f0-9]{8,64})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool TryParse(string? output, out RandomXBenchmarkMeasurement? measurement)
    {
        measurement = null;
        if (string.IsNullOrWhiteSpace(output))
            return false;

        var normalized = AnsiEscapePattern.Replace(output, string.Empty);
        var match = CompletionPattern.Match(normalized);
        if (!match.Success)
            return false;

        if (!decimal.TryParse(match.Groups["seconds"].Value, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var seconds) ||
            !decimal.TryParse(match.Groups["rate"].Value, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var rate) ||
            seconds <= 0m || rate <= 0m)
            return false;

        var multiplier = match.Groups["unit"].Value.ToUpperInvariant() switch
        {
            "H/S" => 1m,
            "KH/S" => 1_000m,
            "MH/S" => 1_000_000m,
            _ => 0m
        };

        if (multiplier == 0m)
            return false;

        measurement = new RandomXBenchmarkMeasurement
        {
            HashrateHps = rate * multiplier,
            ElapsedSeconds = seconds,
            Checksum = match.Groups["checksum"].Value.ToUpperInvariant(),
            CompletedAt = DateTimeOffset.UtcNow
        };
        return true;
    }
}
