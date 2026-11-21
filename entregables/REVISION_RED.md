# Entrega — Numeración central compartida

Estado al 20/11/2026: práctica aislada, consulta de numeración y anulación/reemplazo de la última nota implementados. Instalación y comprobación entre computadoras físicas pendientes.

Revisión de rango: **004026–004225**, alerta desde próximo 004216 (10 disponibles), bloqueo al agotar el rango y actualización desde el pie completo al renovar. Protocolo/registro actuales 4, con migración de 1/2/3. **44 verificaciones generales y 133 de red correctas**. Incluye límites 11/10/1/0, rechazo de rangos inválidos, concurrencia sobre 004225, reintentos, anulación y reemplazo de la última del rango, continuidad tras renovar y persistencia al reiniciar. Captura de Pie de imprenta revisada con rango visible. Pruebas aisladas; sin consumo de números de producción.

Revisión posterior del mismo día: implementado **Pie de imprenta** y control de autorización central, con alerta desde 14 días antes, bloqueo después de caducar y bloqueo de fecha fuera de vigencia. Datos iniciales: autorización 1133313444, emisión 09/03/2026, caducidad 09/03/2027. Renovación solo desde el principal, historial durable y copia en nuevas notas. Protocolo y registro versión 4, con migración de registros 1/2. Guardar el pie no altera el fondo: cargar la imagen del formulario renovado con Cambiar plantilla.

Resultado actual: **44 comprobaciones generales y 116 de red correctas**, más captura de ventana principal y pantalla de pie de imprenta. Se verificaron día 15/14, día de caducidad y posterior, fecha inválida del documento, bloqueo sin consumir número, recuperación de emitidas después de caducar, renovación/reinicio/historial, fallos de guardado y controles de interfaz. También se reproduce un bloqueo de archivo breve y uno persistente: se reintenta únicamente el reemplazo atómico ante errores transitorios de Windows; el bloqueo persistente falla conservando el archivo anterior. No se configuró producción ni se alteraron sus datos para probar.

Actualización: el usuario dispone ahora de un único equipo; el principal previsto tiene Windows 11. 4121 es base de práctica y no el último real confirmado de producción. Abrir_Practica.cmd crea un principal solo local con datos separados, puerto 47832 y siguiente 004122. En producción se mantiene la configuración de roles y el último inicial editable antes de inicializar por primera vez.

Se permite anular y reemplazar solo la última nota, conservando original, motivo, fecha e historial. La numeración no retrocede; el siguiente disponible reutiliza el último número mientras esté anulado. Reintentar la original no la reactiva. Las notas anteriores con otra vigente posterior no liberan números. El PDF de una emitida requiere consultar vigencia en el principal y bloquea las anuladas; archivos previamente exportados no se retiran automáticamente.

Verificación actual: compilación correcta, **44 comprobaciones generales y 84 de red**, más apertura y captura de interfaz con los nuevos botones. Incluye reinicio con reemplazo pendiente, reintentos, dos emisiones concurrentes después de anular, conservación de ambas versiones y fallo durable de anulación. Todo utiliza datos aislados y TCP local; no se inicializó producción ni se configuró su firewall. Se observaron fallos transitorios de persistencia durante las primeras ejecuciones; fueron rechazados por el principal y las dos últimas ejecuciones completas de red finalizaron correctamente.

El protocolo TCP y el registro son versión 4; se migran registros previos. Actualizar todos los equipos juntos y no volver a abrir el registro actualizado con un ejecutable anterior. Los apartados siguientes son la revisión histórica de la primera entrega; prevalece esta actualización.

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
