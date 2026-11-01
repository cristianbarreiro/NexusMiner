# NexusMiner

**MVP de escritorio para Windows — Hardware Analyzer + Profitability Simulator**

NexusMiner es un proyecto educativo para inspeccionar el hardware local y estimar, con supuestos explícitos, si un equipo podría obtener un resultado económico positivo mediante minería de criptomonedas.

> **Alcance de esta primera versión:** no mina, no instala mineros, no se conecta a pools, no crea wallets y no realiza tráfico de red. El simulador utiliza datos que introduce el usuario. El análisis de hardware consulta información local de Windows mediante WMI.

## Funcionalidades incluidas

- Detección de CPU, núcleos, procesadores lógicos, RAM, sistema operativo y adaptadores gráficos mediante WMI.
- Cálculo de monedas esperadas por día a partir de hashrate local, hashrate de red, recompensa por bloque y bloques diarios.
- Estimación mensual de ingreso bruto, conversión a UYU, comisión de pool, consumo eléctrico y resultado neto.
- Cálculo del precio eléctrico de equilibrio estimado.
- Validación de entradas y pruebas unitarias de la lógica financiera.

## Requisitos

- Windows 10/11 de 64 bits.
- .NET 10 SDK (incluye el targeting pack de WPF para Windows al compilar en Windows).
- Conexión a Internet únicamente para restaurar paquetes NuGet la primera vez.
- Visual Studio 2022/2026 con la carga de trabajo **Desarrollo de escritorio de .NET**, o Visual Studio Code con .NET SDK.

La solución apunta a `net10.0-windows`. No se ha fijado una versión de parche específica del SDK: `global.json` permite avanzar a feature bands posteriores de .NET 10.

## Ejecutar

Desde PowerShell, en la carpeta raíz del repositorio:

```powershell
dotnet --info
dotnet restore .\NexusMiner.sln
dotnet test .\NexusMiner.sln --configuration Release
dotnet run --project .\src\NexusMiner.Desktop\NexusMiner.Desktop.csproj
```

También se puede abrir `NexusMiner.sln` en Visual Studio y establecer `NexusMiner.Desktop` como proyecto de inicio.

## Uso del simulador

Para realizar una estimación razonable hay que obtener y revisar estos datos:

1. **Hashrate del equipo (H/s):** medido con un benchmark confiable para el algoritmo elegido. En esta versión se introduce manualmente; NexusMiner todavía no ejecuta benchmarks de minería.
2. **Hashrate de red (H/s):** estimación reciente de la red de la moneda y algoritmo elegidos.
3. **Recompensa por bloque:** recompensa de bloque estimada en unidades de la moneda. Puede variar e incluir comisiones de transacción según la fuente.
4. **Bloques por día:** aproximación basada en el tiempo medio de bloque. El valor `720` corresponde a una red con un bloque cada dos minutos; no debe asumirse para otras redes.
5. **Precio de la moneda (USD)** y **tipo de cambio (UYU por USD)**: introducir cotizaciones actuales de una fuente que el usuario considere fiable.
6. **Consumo (W):** consumo medido del equipo bajo la carga que se quiere modelar. Si se introduce el consumo total del equipo, el costo calculado representa el costo total durante esas horas, no solamente el consumo incremental de la minería.
7. **Tarifa eléctrica (UYU/kWh):** usar el precio marginal que corresponda a la factura y al consumo adicional. Las tarifas, impuestos y cargos fijos pueden cambiar el resultado.

El cálculo es una aproximación matemática de participación esperada:

```text
monedas_por_dia = (hashrate_equipo / hashrate_red)
                  * recompensa_por_bloque
                  * bloques_por_dia

monedas_mes = monedas_por_dia * 30,4375 * (disponibilidad_pct / 100)
ingreso_bruto_UYU = monedas_mes * precio_USD * UYU_por_USD
comision_pool = ingreso_bruto_UYU * (comision_pct / 100)
kWh_mes = (consumo_W / 1000) * 24 * 30,4375 * (disponibilidad_pct / 100)
costo_electrico = kWh_mes * tarifa_UYU_kWh
resultado_neto = ingreso_bruto_UYU - comision_pool - costo_electrico - otros_costos
```

### Cómo interpretar el resultado

- **Resultado neto positivo:** el escenario ingresado sería positivo antes de impuestos, depreciación, fallas, refrigeración no incluida y otros costos omitidos.
- **Resultado neto negativo:** con esos supuestos el equipo costaría más de lo que produciría.
- **Resultado cercano a cero:** la incertidumbre de precios, dificultad, hashrate y consumo puede cambiar fácilmente el signo.
- **Cálculo esperado no es pago real:** la distribución de recompensas, suerte, método de pago del pool, umbral de retiro y comisiones pueden hacer que los pagos observados difieran.

NexusMiner no consulta datos de mercado automáticamente. Los valores de red, recompensas, precios, cambio y tarifas deben actualizarse manualmente antes de tomar decisiones.

## Estructura del repositorio

```text
NexusMiner/
├── NexusMiner.sln
├── global.json
├── Directory.Build.props
├── README.md
├── LICENSE
├── .gitignore
├── docs/
│   ├── ARCHITECTURE.md
│   ├── CALCULATION_MODEL.md
│   ├── ROADMAP.md
│   └── SAFETY_AND_PRIVACY.md
├── scripts/
│   ├── Run-Tests.ps1
│   └── Publish-Windows.ps1
├── src/
│   ├── NexusMiner.Core/
│   │   ├── Models/
│   │   └── Services/
│   ├── NexusMiner.Infrastructure.Windows/
│   └── NexusMiner.Desktop/
│       ├── MainWindow.xaml
│       └── MainWindow.xaml.cs
└── tests/
    └── NexusMiner.Core.Tests/
```

## Limitaciones conocidas del MVP

- La detección utiliza WMI. Algunos fabricantes/drivers pueden devolver nombres incompletos o valores nulos.
- `AdapterRAM` de `Win32_VideoController` puede ser inexacto o estar limitado por WMI; no se utiliza para calcular rentabilidad.
- La detección no mide consumo eléctrico. El consumo debe medirse externamente o introducirse desde una especificación fiable.
- No se ejecutan benchmarks RandomX, SHA-256, Etchash ni ningún otro algoritmo.
- No se realizan cálculos fiscales ni se modelan depreciación, refrigeración, retiro mínimo, variaciones de dificultad ni volatilidad intradía.
- La app no es asesoramiento financiero. No compres hardware ni mantengas un equipo minando basándote únicamente en la simulación.

## Pruebas

```powershell
dotnet test .\NexusMiner.sln --configuration Release
```

La lógica de cálculo está en `NexusMiner.Core`, independiente de WPF y WMI, para que sus pruebas puedan ejecutarse en cualquier sistema con el SDK de .NET 10.

## Licencia

MIT. Ver `LICENSE`.
