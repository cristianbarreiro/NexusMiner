using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using NexusMiner.Core.Models;

namespace NexusMiner.Infrastructure.Windows;

public sealed record XMRigBenchmarkExecutionResult(
    int ExitCode,
    string Output,
    bool TerminatedAfterCompletion);

/// <summary>
/// Runs XMRig only in local offline benchmark mode. Pool, wallet, URL, config,
/// submission, and arbitrary arguments are intentionally not supported.
/// </summary>
public sealed class XMRigBenchmarkRunner
{
    public async Task<XMRigBenchmarkExecutionResult> RunOfflineAsync(
        string executablePath,
        RandomXBenchmarkOptions options,
        Action<string>? outputReceived = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate(Environment.ProcessorCount);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException("Selecciona xmrig.exe.", nameof(executablePath));

        var executable = Path.GetFullPath(executablePath);
        if (!string.Equals(Path.GetFileName(executable), "xmrig.exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El ejecutable debe llamarse xmrig.exe.", nameof(executablePath));
        if (!File.Exists(executable))
            throw new FileNotFoundException("No se encontró xmrig.exe.", executable);

        var workingDirectory = Path.Combine(Path.GetTempPath(), "NexusMiner",
            "OfflineBenchmark", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            // Fixed offline-only arguments. Never add --submit, --url, --user or --config.
            startInfo.ArgumentList.Add($"--bench={options.NormalizedBenchmarkSize}");
            startInfo.ArgumentList.Add("--algo=rx/0");
            startInfo.ArgumentList.Add($"--threads={options.ThreadCount}");
            startInfo.ArgumentList.Add("--cpu-priority=1");
            startInfo.ArgumentList.Add("--randomx-mode=fast");
            startInfo.ArgumentList.Add("--randomx-init=1");
            startInfo.ArgumentList.Add("--no-huge-pages");
            startInfo.ArgumentList.Add("--no-color");

            var output = new StringBuilder();
            var outputLock = new object();
            var benchmarkFinished = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            using var process = new Process { StartInfo = startInfo };

            void CaptureLine(string? line)
            {
                if (line is null)
                    return;

                lock (outputLock)
                    output.AppendLine(line);

                try { outputReceived?.Invoke(line); }
                catch { /* UI callbacks must not interrupt output draining. */ }

                if (line.Contains("benchmark finished in", StringComparison.OrdinalIgnoreCase))
                    benchmarkFinished.TrySetResult(true);
            }

            process.OutputDataReceived += (_, e) => CaptureLine(e.Data);
            process.ErrorDataReceived += (_, e) => CaptureLine(e.Data);

            if (!process.Start())
                throw new InvalidOperationException("Windows no pudo iniciar xmrig.exe.");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var registration = cancellationToken.Register(() => TryKill(process));
            var terminatedAfterCompletion = false;

            try
            {
                var exitTask = process.WaitForExitAsync(cancellationToken);
                var finishedTask = await Task.WhenAny(exitTask, benchmarkFinished.Task);

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationToken);

                if (finishedTask == benchmarkFinished.Task)
                {
                    // XMRig waits for Ctrl+C after printing the completed benchmark summary.
                    terminatedAfterCompletion = true;
                    TryKill(process);
                    await process.WaitForExitAsync(CancellationToken.None);
                }
                else
                {
                    await exitTask;
                }
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                await process.WaitForExitAsync(CancellationToken.None);
                throw;
            }

            process.WaitForExit(); // Drain redirected output events.
            string completeOutput;
            lock (outputLock)
                completeOutput = output.ToString();

            return new XMRigBenchmarkExecutionResult(
                process.ExitCode, completeOutput, terminatedAfterCompletion);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workingDirectory))
                    Directory.Delete(workingDirectory, recursive: true);
            }
            catch (IOException) { /* Best-effort cleanup. */ }
            catch (UnauthorizedAccessException) { /* Best-effort cleanup. */ }
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) { }
        catch (Win32Exception) { }
        catch (NotSupportedException) { }
    }
}
