# Entrega — Numeración central compartida

Estado: implementación y paquete preparados; instalación en el principal físico y comprobación entre computadoras pendientes.

## Confirmado por el usuario

- La impresión ya se realiza al tamaño correcto.
- El principal será otro computador, no el de esta sesión.
- El último número utilizado es 4121; la próxima emisión prevista es 004122.

## Incluido

- Bodoni.Red.exe: el mismo programa puede configurarse como principal o como secundario.
- Configuración y exportación de un archivo de conexión para los secundarios.
- Numeración asignada únicamente por el principal, con guardado conjunto del documento y consecutivo.
- Identificación persistente de cada solicitud para no duplicar emisiones por doble clic o pérdida de respuesta.
- Recuperación de emisiones pendientes después de reiniciar el cliente o reconectar.
- Historial central para recuperar y reimprimir notas sin cambiar su número.
- Dos modalidades PDF existentes, con el número emitido en ambas.
- Herramienta de firewall para ejecutar exclusivamente en el principal elegido, limitada a red privada y subred local.
- Paquete ZIP que no incluye datos privados, claves preconfiguradas ni un contador independiente para cada equipo.

## Evidencia

- VERIFICACION.txt: regresión de cálculos, plantilla, PDF y borradores.
- VERIFICACION_RED.txt: pruebas con servidor y tres clientes TCP en el mismo equipo, incluidos fallos de conexión y persistencia.
- SIMULACION-RED-nota-numerada.pdf: muestra generada con datos aislados de prueba; no es una emisión real de Bodoni.

Se comprueba la lógica de concurrencia usando tres conexiones TCP independientes. Esto no equivale a haber configurado ni probado los otros computadores físicos. No se abrió un servicio principal real ni se modificó el firewall de este equipo para esa instalación.

Resultado de la revisión: **44 comprobaciones de regresión y 63 de red correctas**. El PDF numerado de simulación se abrió correctamente en el motor Ghostscript. Los scripts de compilación, verificación y firewall pasaron la comprobación de sintaxis; la regla de firewall está preparada para ejecutarse en el otro computador principal, y no se ha aplicado aquí.

## Para revisar con el usuario

- [ ] Extraer el ZIP en una carpeta fija del principal elegido.
- [ ] Configurarlo como principal con último número 4121.
- [ ] Habilitar la entrada de Bodoni en la red privada del principal.
- [ ] Exportar e importar la conexión en los secundarios.
- [ ] Ver el mismo último número desde dos computadores, sin consumir numeración.
- [ ] Emitir la siguiente venta real y comprobar que todos consultan el nuevo último número.
- [ ] Recuperar esa nota desde Historial y generar su PDF con el mismo número.

El cotizador, anulación de notas y restauración asistida siguen fuera de esta entrega. Las notas emitidas son inmutables; una recuperación de datos antiguos requiere reconciliar la numeración antes de seguir emitiendo.
