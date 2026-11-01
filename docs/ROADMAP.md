# Roadmap

## MVP 0.1 — completado
- [x] Detección local de CPU, RAM, GPU y sistema operativo mediante WMI.
- [x] Simulador financiero con parámetros manuales y cálculos deterministas.
- [x] Validación y pruebas unitarias de las fórmulas.

## MVP 0.2 — benchmark offline
- [x] Pestaña CPU Benchmark en WPF.
- [x] Integración opt-in con el modo benchmark offline de XMRig.
- [x] Selección explícita de xmrig.exe; no se descarga ni instala automáticamente.
- [x] Parámetros restringidos a RandomX rx/0, tamaños 250K/500K/1M y selección de hilos.
- [x] Prioridad baja y huge pages desactivadas.
- [x] Cancelación y cierre automático al detectar el resumen final.
- [x] Parser de hashrate, duración y checksum con pruebas unitarias.
- [ ] Validar compilación y ejecución de extremo a extremo en Windows con XMRig oficial.

## MVP 0.3 — medición y análisis
- [ ] Historial local de resultados de benchmark en SQLite.
- [ ] Asociar resultados con snapshots de hardware y configuración de hilos.
- [ ] Importar el hashrate medido al Profitability Simulator.
- [ ] Añadir escenarios conservador/base/optimista.
- [ ] Medición de consumo con medidor externo e importación manual.
- [ ] Telemetría térmica solo mediante sensores compatibles.

## MVP 0.4 — evaluación de minería real (sujeta a decisión)
- [ ] Revisar licencias, versiones y hashes de cualquier motor externo.
- [ ] Mantener la minería deshabilitada por defecto y exigir consentimiento explícito.
- [ ] No añadir pools ni wallets predeterminados.
- [ ] Añadir límites de temperatura, consumo, tiempo y parada de emergencia.
- [ ] Evaluar rentabilidad actualizada antes de habilitar cualquier operación.

## MVP 1.0
- [ ] Cotizaciones y datos de red con fuente y marca temporal.
- [ ] Tarifas eléctricas configurables por tramo.
- [ ] Recomendar no operar cuando los datos sean insuficientes o el beneficio neto esperado sea negativo.
