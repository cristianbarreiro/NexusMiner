# Modelo de cálculo

El simulador calcula un **valor esperado simplificado** usando la participación del hashrate del dispositivo sobre el hashrate total de la red.

## Variables

- `H_device`: hashrate del equipo, en hashes por segundo.
- `H_network`: hashrate estimado de toda la red, en hashes por segundo.
- `R_block`: recompensa estimada por bloque, en monedas.
- `B_day`: bloques estimados por día.
- `P_coin`: precio de una moneda en USD.
- `FX`: UYU por USD.
- `W`: potencia eléctrica promedio bajo carga, en watts.
- `T`: tarifa marginal, en UYU/kWh.
- `U`: disponibilidad, de 0 a 100 por ciento.
- `F`: comisión del pool, de 0 a 100 por ciento.

## Fórmulas

```text
coins_day = (H_device / H_network) * R_block * B_day
uptime = U / 100
coins_month = coins_day * 30.4375 * uptime
gross_USD = coins_month * P_coin
gross_UYU = gross_USD * FX
pool_fee_UYU = gross_UYU * F / 100
energy_kWh = (W / 1000) * 24 * 30.4375 * uptime
electricity_UYU = energy_kWh * T
net_UYU = gross_UYU - pool_fee_UYU - electricity_UYU - other_monthly_costs
break_even_tariff = (gross_UYU - pool_fee_UYU - other_monthly_costs) / energy_kWh
```

## Supuestos y límites

- Hasrate y dificultad de red se consideran constantes durante el periodo.
- La recompensa por bloque y el tiempo medio de bloque se consideran constantes.
- Se ignoran bloques huérfanos, variaciones de suerte, cambios de dificultad, comisiones de transacción variables, esquema de pago del pool, mínimo de retiro y conversión al retirar.
- El cálculo no predice el momento ni el importe de pagos reales.
- Los 30,4375 días son una convención para representar un mes promedio.
- El precio eléctrico debe corresponder al cargo marginal real para el consumo adicional. Cargos fijos ya existentes no deberían asignarse enteros al minero.
- La potencia debe ser la del equipo completo bajo la condición modelada. No confundir TDP del procesador con consumo de la notebook en el enchufe.
