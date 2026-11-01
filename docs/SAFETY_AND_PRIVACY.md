# Seguridad y privacidad

- La versión 0.1 no mina, no instala servicios, no se ejecuta al iniciar Windows y no solicita privilegios de administrador.
- El análisis de hardware consulta WMI local; no envía el inventario a ningún servidor.
- No se guardan wallets, claves privadas, credenciales ni direcciones de pago.
- No se realizan llamadas a Internet desde la aplicación.
- No se descargan ejecutables de minería.
- El simulador requiere que el usuario proporcione cotizaciones y parámetros. La precisión del resultado depende de esos datos.
- No ejecutar archivos de minería de terceros sin revisar origen, firma/hash y permisos.
- Para cualquier futura integración de minería se requerirá una acción explícita por parte del usuario y controles de parada visibles.
