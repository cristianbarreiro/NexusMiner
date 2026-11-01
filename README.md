# NexusMiner

**Aplicación de escritorio para Windows — Hardware Analyzer + Profitability Simulator + RandomX Offline Benchmark**

NexusMiner es un proyecto educativo para inspeccionar el hardware local, medir el rendimiento de CPU mediante un benchmark offline y estimar escenarios de rentabilidad con supuestos explícitos.

> **Alcance de esta versión:** no realiza minería real, no se conecta a pools, no crea wallets y no envía shares ni resultados. El benchmark RandomX usa el modo offline de un XMRig que el usuario selecciona manualmente.

## Funcionalidades

- Detección local de CPU, núcleos, procesadores lógicos, RAM, sistema operativo y adaptadores gráficos mediante WMI.
- Simulador financiero con ingresos esperados, conversión a UYU, consumo eléctrico, comisiones, resultado neto y tarifa eléctrica de equilibrio.
- Benchmark RandomX offline opt-in con selección explícita de `xmrig.exe`, tamaño y número de hilos.
- Captura de hashrate, duración y checksum del resumen del benchmark.
- Cancelación del proceso y cierre automático tras detectar el resumen final.
- Pruebas unitarias para fórmulas, validación de opciones y parsing de resultados.

## Requisitos

- Windows 10/11 de 64 bits.
- .NET 10 SDK.
- Visual Studio con la carga de trabajo Desarrollo de escritorio de .NET o VS Code + SDK.
- XMRig oficial, solo si se quiere usar la pestaña CPU Benchmark.

## Ejecutar

Desde PowerShell, en la carpeta raíz:

```powershell
dotnet restore .\NexusMiner.sln
dotnet test .\NexusMiner.sln --configuration Release
dotnet run --project .\src\NexusMiner.Desktop\NexusMiner.Desktop.csproj
```

## Benchmark RandomX offline

1. Descarga XMRig únicamente desde el [repositorio oficial](https://github.com/xmrig/xmrig/releases) y verifica el origen del ejecutable.
2. Abre la pestaña **CPU Benchmark** y selecciona `xmrig.exe`.
3. Comienza con 1 o 2 hilos y tamaño `250K`; compara después 3 y 4 hilos.
4. `250K` significa 250.000 hashes, no 250 segundos. La duración depende del rendimiento de CPU y puede ser de varios minutos.
5. NexusMiner ejecuta únicamente RandomX `rx/0` offline, prioridad baja, modo fast y huge pages desactivadas. No pasa URL de pool, wallet, archivo de configuración ni `--submit`.
6. El proceso se cierra al detectar el resumen final o al cancelar. El checksum se muestra para inspección; no se verifica independientemente.

El benchmark puede elevar la utilización y temperatura de CPU. Ejecuta con ventilación adecuada, con la notebook conectada a la corriente y detén la prueba ante temperaturas anormales. No desactives el antivirus para ejecutar XMRig.

## Uso del simulador

Para una estimación razonable hay que introducir:
1. Hashrate del equipo medido para el algoritmo elegido.
2. Hashrate actual de red.
3. Recompensa estimada por bloque y bloques por día.
4. Precio actual de la moneda y tipo de cambio UYU/USD.
5. Consumo eléctrico del equipo bajo carga y tarifa marginal real de UTE.
6. Disponibilidad, comisión del pool y otros costos.

El cálculo representa valor esperado simplificado, no pagos garantizados. Hashrate y dificultad de red se consideran constantes durante el periodo. No se modelan impuestos, depreciación, refrigeración, suerte del pool, mínimo de retiro ni volatilidad.

## Limitaciones conocidas

- No hay minería real ni integración con pools.
- No se descargan ejecutables automáticamente.
- El benchmark offline no genera criptomonedas.
- No se registran todavía resultados de benchmark en SQLite ni se importan automáticamente al simulador.
- No se mide consumo eléctrico ni temperatura automáticamente.
- El checksum se captura para inspección, pero no se verifica independientemente.
- WMI puede reportar información incompleta o aproximada, especialmente la memoria de GPU.

## Estructura

```text
NexusMiner/
├── NexusMiner.sln
├── docs/
├── scripts/
├── src/
│   ├── NexusMiner.Core/
│   ├── NexusMiner.Infrastructure.Windows/
│   └── NexusMiner.Desktop/
└── tests/
    └── NexusMiner.Core.Tests/
```

## Pruebas

```powershell
dotnet test .\NexusMiner.sln --configuration Release
```

## Seguridad y privacidad

Hardware Analyzer consulta WMI local. El benchmark solo se ejecuta tras una selección explícita del usuario y no requiere privilegios de administrador. El proyecto no guarda wallets, claves privadas ni credenciales. Ver [docs/SAFETY_AND_PRIVACY.md](docs/SAFETY_AND_PRIVACY.md).

## Licencia

MIT. Ver `LICENSE`.
