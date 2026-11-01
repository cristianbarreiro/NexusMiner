using System.Globalization;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows;
using NexusMiner.Core.Models;
using NexusMiner.Core.Services;
using NexusMiner.Infrastructure.Windows;

namespace NexusMiner.Desktop;

public partial class MainWindow : Window
{
    private readonly HardwareAnalyzer _hardwareAnalyzer = new();
    private readonly ProfitabilityCalculator _calculator = new();

    private readonly XMRigBenchmarkRunner _benchmarkRunner = new();
    private CancellationTokenSource? _benchmarkCancellation;

    private void SelectXMRigButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar XMRig",
            Filter = "XMRig executable (xmrig.exe)|xmrig.exe|Executable files (*.exe)|*.exe",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
            return;

        if (!string.Equals(Path.GetFileName(dialog.FileName), "xmrig.exe", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkStatusText.Text = "Selecciona el archivo llamado xmrig.exe descargado desde el repositorio oficial de XMRig.";
            return;
        }

        XMRigPathBox.Text = dialog.FileName;
        BenchmarkStatusText.Text = "Ejecutable seleccionado. La prueba será local y offline.";
    }

    private async void RunRandomXBenchmarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (_benchmarkCancellation is not null)
            return;

        var executablePath = XMRigPathBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
        {
            BenchmarkStatusText.Text = "Selecciona primero un xmrig.exe existente.";
            return;
        }

        var size = (BenchmarkSizeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "250K";
        var threadText = (BenchmarkThreadsBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "1";
        if (!int.TryParse(threadText, NumberStyles.None, CultureInfo.InvariantCulture, out var threadCount))
        {
            BenchmarkStatusText.Text = "La cantidad de hilos seleccionada no es válida.";
            return;
        }

        var options = new RandomXBenchmarkOptions { BenchmarkSize = size, ThreadCount = threadCount };
        try
        {
            options.Validate(Environment.ProcessorCount);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            BenchmarkStatusText.Text = ex.Message;
            return;
        }

        var benchmarkCancellation = new CancellationTokenSource();
        _benchmarkCancellation = benchmarkCancellation;
        RunRandomXBenchmarkButton.IsEnabled = false;
        StopRandomXBenchmarkButton.IsEnabled = true;
        SelectXMRigButton.IsEnabled = false;
        BenchmarkLogTextBox.Clear();
        BenchmarkHashrateResult.Text = "—";
        BenchmarkElapsedResult.Text = "—";
        BenchmarkChecksumResult.Text = "—";
        BenchmarkStatusText.Text = $"Ejecutando RandomX rx/0 · {options.NormalizedBenchmarkSize} · {options.ThreadCount} hilo(s). Puedes cancelar en cualquier momento.";
        AppendBenchmarkOutput($"[{DateTime.Now:G}] Inicio de benchmark offline. No se enviarán resultados.");

        try
        {
            var execution = await _benchmarkRunner.RunOfflineAsync(
                executablePath,
                options,
                AppendBenchmarkOutput,
                benchmarkCancellation.Token);

            if (RandomXBenchmarkOutputParser.TryParse(execution.Output, out var measurement) && measurement is not null)
            {
                BenchmarkHashrateResult.Text = $"{measurement.HashrateHps:N1} H/s";
                BenchmarkElapsedResult.Text = $"{measurement.ElapsedSeconds:N2} s";
                BenchmarkChecksumResult.Text = measurement.Checksum;
                BenchmarkStatusText.Text = execution.TerminatedAfterCompletion || execution.ExitCode == 0
                    ? "Benchmark completado. El proceso se cerró al terminar el cálculo; no se publicó ningún resultado."
                    : $"Se obtuvo un resumen, pero XMRig terminó con código {execution.ExitCode}. Revisa el registro.";
            }
            else
            {
                BenchmarkStatusText.Text = $"No se encontró el resumen final de XMRig (código {execution.ExitCode}). Revisa el registro y la versión del ejecutable.";
            }
        }
        catch (OperationCanceledException)
        {
            BenchmarkStatusText.Text = "Benchmark cancelado. El proceso de XMRig fue detenido.";
            AppendBenchmarkOutput($"[{DateTime.Now:G}] Prueba cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            BenchmarkStatusText.Text = $"No se pudo ejecutar el benchmark: {ex.Message}";
            AppendBenchmarkOutput($"ERROR: {ex}");
        }
        finally
        {
            benchmarkCancellation.Dispose();
            if (ReferenceEquals(_benchmarkCancellation, benchmarkCancellation))
                _benchmarkCancellation = null;

            RunRandomXBenchmarkButton.IsEnabled = true;
            StopRandomXBenchmarkButton.IsEnabled = false;
            SelectXMRigButton.IsEnabled = true;
        }
    }

    private void StopRandomXBenchmarkButton_Click(object sender, RoutedEventArgs e)
    {
        _benchmarkCancellation?.Cancel();
    }

    private void AppendBenchmarkOutput(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(new Action(() => AppendBenchmarkOutput(line)));
            return;
        }

        BenchmarkLogTextBox.AppendText(line + Environment.NewLine);
        BenchmarkLogTextBox.ScrollToEnd();
    }


    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RefreshHardwareButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshHardwareButton.IsEnabled = false;
        HardwareWarningsText.Text = "Consultando WMI...";

        try
        {
            var snapshot = await _hardwareAnalyzer.GetSnapshotAsync();
            OperatingSystemText.Text = snapshot.OperatingSystem;
            ProcessorText.Text = snapshot.ProcessorName;
            CoresText.Text = snapshot.PhysicalCores.ToString(CultureInfo.InvariantCulture);
            LogicalProcessorsText.Text = snapshot.LogicalProcessors.ToString(CultureInfo.InvariantCulture);
            MemoryText.Text = snapshot.TotalMemoryGb > 0m ? $"{snapshot.TotalMemoryGb:N1} GB" : "No detectada";
            GpuText.Text = snapshot.Gpus.Count == 0
                ? "No se detectaron adaptadores gráficos."
                : string.Join(Environment.NewLine, snapshot.Gpus.Select(g =>
                    g.AdapterMemoryGb.HasValue
                        ? $"• {g.Name} ({g.AdapterMemoryGb.Value:N1} GB reportados por WMI)"
                        : $"• {g.Name} (memoria no informada)"));
            HardwareTimestampText.Text = $"Última lectura: {snapshot.CapturedAt.ToLocalTime():G}";
            HardwareWarningsText.Text = snapshot.Warnings.Count == 0
                ? "Lectura completada. WMI puede omitir o reportar valores aproximados según el driver."
                : string.Join(Environment.NewLine, snapshot.Warnings);
        }
        catch (Exception ex)
        {
            HardwareWarningsText.Text = $"No se pudo completar el análisis: {ex.Message}";
        }
        finally
        {
            RefreshHardwareButton.IsEnabled = true;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculationErrorText.Text = string.Empty;

        try
        {
            var input = new ProfitabilityInput
            {
                DeviceHashrateHps = ParseDecimal(DeviceHashrateBox.Text, "Hashrate del equipo"),
                NetworkHashrateHps = ParseDecimal(NetworkHashrateBox.Text, "Hashrate de red"),
                BlockRewardCoins = ParseDecimal(BlockRewardBox.Text, "Recompensa por bloque"),
                BlocksPerDay = ParseDecimal(BlocksPerDayBox.Text, "Bloques por día"),
                CoinPriceUsd = ParseDecimal(CoinPriceBox.Text, "Precio de la moneda"),
                UyuPerUsd = ParseDecimal(UyuPerUsdBox.Text, "Tipo de cambio"),
                PowerWatts = ParseDecimal(PowerWattsBox.Text, "Consumo eléctrico"),
                ElectricityPriceUyuPerKwh = ParseDecimal(ElectricityRateBox.Text, "Tarifa eléctrica"),
                UptimePercent = ParseDecimal(UptimeBox.Text, "Disponibilidad"),
                PoolFeePercent = ParseDecimal(PoolFeeBox.Text, "Comisión del pool"),
                OtherMonthlyCostsUyu = ParseDecimal(OtherCostsBox.Text, "Otros costos")
            };

            var result = _calculator.Calculate(input);
            CoinsPerDayResult.Text = result.ExpectedCoinsPerDay.ToString("0.##########", CultureInfo.CurrentCulture);
            GrossResult.Text = FormatUyu(result.GrossRevenueUyuPerMonth);
            EnergyResult.Text = $"{result.EnergyKwhPerMonth:N2}";
            ElectricityResult.Text = FormatUyu(result.ElectricityCostUyuPerMonth);
            PoolFeeResult.Text = FormatUyu(result.PoolFeeUyuPerMonth);
            NetResult.Text = FormatUyu(result.NetUyuPerMonth);
            BreakEvenText.Text = result.BreakEvenElectricityPriceUyuPerKwh.HasValue
                ? $"Precio eléctrico de equilibrio: {result.BreakEvenElectricityPriceUyuPerKwh.Value:N4} UYU/kWh"
                : "Precio eléctrico de equilibrio: no disponible (consumo mensual igual a cero).";

            if (input.DeviceHashrateHps == 0m || input.BlockRewardCoins == 0m || input.CoinPriceUsd == 0m)
            {
                ProfitabilityStatusText.Text = "Faltan datos de rendimiento o mercado: el ingreso calculado puede ser cero. Completa los valores actuales antes de interpretar el resultado.";
            }
            else if (result.NetUyuPerMonth > 0m)
            {
                ProfitabilityStatusText.Text = "El escenario ingresado muestra un resultado neto positivo, antes de costos omitidos, impuestos, depreciación y variaciones de mercado/red.";
            }
            else if (result.NetUyuPerMonth < 0m)
            {
                ProfitabilityStatusText.Text = "Con estos supuestos, el escenario presenta pérdida mensual estimada. No mantengas el equipo operando basándote solo en esta proyección.";
            }
            else
            {
                ProfitabilityStatusText.Text = "El escenario queda en equilibrio según los datos introducidos. La incertidumbre puede cambiar el resultado.";
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            CalculationErrorText.Text = ex.Message;
        }
    }

    private static decimal ParseDecimal(string? text, string fieldName)
    {
        var normalized = (text ?? string.Empty).Trim().Replace(" ", string.Empty);
        if (normalized.Contains(',') && normalized.Contains('.'))
        {
            var decimalSeparator = normalized.LastIndexOf(',') > normalized.LastIndexOf('.') ? ',' : '.';
            var thousandsSeparator = decimalSeparator == ',' ? '.' : ',';
            normalized = normalized.Replace(thousandsSeparator.ToString(), string.Empty)
                                   .Replace(decimalSeparator, '.');
        }
        else
        {
            normalized = normalized.Replace(',', '.');
        }

        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"'{fieldName}' debe ser un número válido. Puedes usar coma o punto decimal, sin separadores de miles.");
        }

        return value;
    }

    private static string FormatUyu(decimal value) => $"$ {value:N2} UYU";
}
