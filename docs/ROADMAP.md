# Roadmap

## MVP 0.1 — incluido

- [x] Detección local de CPU, RAM, GPU y sistema operativo mediante WMI.
- [x] Simulador con inputs manuales y cálculos deterministas.
- [x] Cálculo de monedas esperadas, ingresos, energía, comisión y neto.
- [x] Pruebas unitarias de validación y fórmulas.
- [x] Aplicación WPF para Windows.

## MVP 0.2 — medir antes de minar

- [ ] Historial local de snapshots de hardware.
- [ ] Medición de consumo por medidor externo o input manual documentado.
- [ ] Benchmark CPU no relacionado con minería para verificar estabilidad térmica.
- [ ] Exportación CSV de escenarios.
- [ ] Comparación de escenarios conservador/base/optimista.

## MVP 0.3 — benchmark de algoritmo

- [ ] Integrar un benchmark explícito y opt-in para un algoritmo elegido.
- [ ] Mostrar temperatura y frecuencia solo cuando exista una fuente de telemetría compatible.
- [ ] Límites de duración, temperatura y uso configurables.
- [ ] No iniciar benchmarks de forma automática ni al arrancar Windows.

## MVP 0.4 — minería real (solo tras revisión)

- [ ] Evaluar integración opcional con un minero externo conocido.
- [ ] Descarga verificable, versión fija y checksum del binario.
- [ ] Consentimiento explícito antes de ejecutar procesos de minería.
- [ ] Pool, wallet y comisión siempre configurados por el usuario.
- [ ] Botón de parada, límites térmicos y registro de eventos.

## MVP 1.0

- [ ] Integración de cotizaciones con fuente configurable y marca temporal.
- [ ] Tarifas eléctricas configurables por tramo.
- [ ] Recomendación de no operar cuando el neto esperado sea negativo o la calidad de datos sea insuficiente.
