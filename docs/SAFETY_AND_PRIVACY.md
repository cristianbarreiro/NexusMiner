# Seguridad y privacidad

- La versión 0.1 no mina, no instala servicios, no se ejecuta al iniciar Windows y no solicita privilegios de administrador.
- El análisis de hardware consulta WMI local; no envía el inventario a ningún servidor.
- No se guardan wallets, claves privadas, credenciales ni direcciones de pago.
- No se realizan llamadas a Internet desde la aplicación.
- No se descargan ejecutables de minería.
- El simulador requiere que el usuario proporcione cotizaciones y parámetros. La precisión del resultado depende de esos datos.
- No ejecutar archivos de minería de terceros sin revisar origen, firma/hash y permisos.
- Para cualquier futura integración de minería se requerirá una acción explícita por parte del usuario y controles de parada visibles.


## Benchmark offline (0.2)
- El benchmark no genera monedas, no envía shares y no se conecta a pools.
- Solo se ejecuta después de que el usuario selecciona manualmente xmrig.exe y pulsa Iniciar.
- NexusMiner construye los argumentos de forma fija y no expone un campo para argumentos arbitrarios.
- No se utiliza --submit, --url, --user, --config ni credenciales.
- Se inicia sin privilegios elevados, con prioridad CPU baja y huge pages desactivadas.
- El usuario puede cancelar la prueba; el proceso se termina al detectar el resumen final.
- El usuario debe obtener XMRig desde su repositorio oficial y no debe desactivar el antivirus ante alertas.
- El benchmark utiliza CPU intensivamente durante varios minutos; vigilar ventilación y temperatura.
- El checksum se muestra, pero NexusMiner no lo verifica de forma independiente.
