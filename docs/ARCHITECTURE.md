# Arquitectura

## Capas

- **NexusMiner.Core:** modelos y lógica de simulación sin dependencias de Windows, WPF ni red.
- **NexusMiner.Infrastructure.Windows:** detección de hardware local mediante WMI (`System.Management`).
- **NexusMiner.Desktop:** interfaz WPF y orquestación de las acciones del usuario.
- **NexusMiner.Core.Tests:** pruebas unitarias del motor financiero.

## Flujo de datos

1. El usuario pulsa **Analizar hardware**.
2. `HardwareAnalyzer` consulta clases WMI locales (`Win32_Processor`, `Win32_ComputerSystem`, `Win32_OperatingSystem`, `Win32_VideoController`).
3. La UI presenta los valores y advertencias, sin iniciar pruebas de carga.
4. En el simulador, el usuario introduce datos de red, mercado, electricidad y operación.
5. `ProfitabilityCalculator` valida y calcula el escenario de forma determinista.
6. La UI presenta valores esperados y el resultado neto estimado.

## Decisiones

- WPF se utiliza por su integración nativa con Windows y .NET.
- La lógica financiera no depende de WPF ni de APIs externas, lo que facilita pruebas y futura reutilización.
- No hay proceso de minería, integración de wallet ni conexión a pool en el MVP.
- No se persisten datos ni se recopila telemetría. Cada cálculo permanece en memoria.


## Benchmark RandomX offline (0.2)
- NexusMiner.Core contiene las opciones validadas y el parser del resumen de XMRig.
- NexusMiner.Infrastructure.Windows ejecuta un proceso local aislado mediante ProcessStartInfo.ArgumentList; no usa shell ni acepta argumentos arbitrarios.
- NexusMiner.Desktop requiere que el usuario seleccione explícitamente xmrig.exe, tamaño e hilos.
- La lista de argumentos está restringida al benchmark offline rx/0; no incluye URL de pool, wallet, configuración externa ni --submit.
- El proceso se cancela bajo demanda y se cierra cuando XMRig emite el resumen final.
