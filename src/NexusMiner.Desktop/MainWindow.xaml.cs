using System.Globalization;
using System.Windows;
using NexusMiner.Core.Models;
using NexusMiner.Core.Services;
using NexusMiner.Infrastructure.Windows;

namespace NexusMiner.Desktop;

public partial class MainWindow : Window
{
    private readonly HardwareAnalyzer _hardwareAnalyzer = new();
    private readonly ProfitabilityCalculator _calculator = new();

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
