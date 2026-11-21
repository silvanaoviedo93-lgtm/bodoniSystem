# Plan de desarrollo — Bodoni para Windows

Estado: impresión al tamaño correcto confirmada por el usuario. Se adelantó la sincronización por red antes del cotizador. La versión de red está implementada y probada con clientes TCP locales; la instalación en los computadores físicos está pendiente.

## Actualización vigente — 20/11/2026

Control adicional de rango: autorizado 004026–004225 según el pie. Alerta al quedar 10 números disponibles (próximo 004216); bloqueo al agotarse o quedar fuera del rango. La renovación incluye rango y fechas, mantiene contador e historial. El principal valida también ante emisiones concurrentes. Protocolo y registro actuales: versión 4, con migración de registros anteriores.

Se añade control central de autorización y pie de imprenta: AUT. S.R.I. 1133313444, emisión 09/03/2026, caducidad 09/03/2027. Aviso desde 23/02/2027 (14 días antes), permiso hasta 09/03/2027 inclusive y bloqueo desde 10/03/2027. La fecha de la nota también debe quedar dentro de la vigencia. Pantalla Pie de imprenta para registrar renovación con historial, sin reiniciar la secuencia. Los secundarios consultan la misma vigencia; recuperar una nota emitida previamente sigue permitido. No hay alertas con el programa cerrado. Actualizar todos los equipos juntos: protocolo y registro versión 4.

El usuario dispone ahora de un único computador; el principal previsto tiene Windows 11. Se incorpora práctica aislada mediante Abrir_Practica.cmd (último inicial 004121, siguiente 004122), configuración de roles para producción y consulta explícita de numeración. Por decisión posterior, la última nota puede anularse y reemplazarse con el mismo número, conservando la original, motivo, fecha e historial. No se reutilizan números de notas que tienen otra vigente posterior. El último real de producción es desconocido y debe ingresarse antes de inicializar el principal. Estas decisiones sustituyen las reglas históricas de no reutilización que aparecen debajo. La validación entre equipos físicos sigue pendiente.

## 1. Objetivo y orden de trabajo

La primera versión permitirá crear notas de venta en una computadora con Windows, ingresar sus valores manualmente, guardar los documentos y generar un PDF para imprimir los datos y la secuencia sobre los formularios preimpresos de Bodoni.

El orden acordado es:

1. Notas de venta y PDF en una computadora.
2. Consecutivo compartido desde 2 o 3 computadores, con otro equipo como principal.
3. Cotizador, después de validar la nota de venta y su numeración compartida.

Cada etapa tendrá un entregable que se pueda revisar y criterios de aceptación. Las observaciones se resolverán antes de cerrar la etapa. Las funciones previstas para después no serán requisitos para entregar la primera versión local.

## 2. Primera versión: notas de venta en una computadora

- Aplicación de escritorio para Windows. La primera entrega fue local; la entrega actual añade un principal dentro de la red de la empresa, sin requerir internet.
- Creación directa de una nota de venta, sin depender de una cotización.
- Datos del cliente: nombre, identificación, dirección, teléfono y correo; fecha de emisión del documento.
- Ingreso manual de cantidad, descripción y precio unitario por concepto.
- Cálculo del importe de cada línea y del total en dólares.
- Guardado y recuperación de borradores sin consumir numeración.
- Emisión de notas con serie y consecutivo guardados conjuntamente en el principal, compartidos por los demás equipos.
- Vista previa y generación de PDF de 200 × 145 mm: completo con plantilla para revisión o papel blanco, y solo datos para papel preimpreso.
- Historial local para consultar notas, regenerar su PDF y reimprimir sin cambiar su número.
- Anulación conservando el documento y su número.
- Prueba de alineación sin consumir números, y ajustes de impresión por perfil.
- Copia de seguridad y restauración de documentos, configuración y numeración.
- Instalador para el equipo de uso y guía breve.

El primer entregable funcional será la nota de venta con precios manuales y PDF. El cálculo de costos de materiales, tamaños, acabados, desperdicio o margen de ganancia queda para futuras ampliaciones.

### Numeración local y referencia recibida

La imagen más reciente muestra estos elementos separados:

| Elemento | Referencia |
|---|---|
| Texto de serie | SERIE 001-001-000 |
| Número de una nota ya emitida | 004026 |
| Presentación del número | Seis dígitos con ceros a la izquierda; color rojo en la muestra. |
| Último consecutivo informado para continuar | 4121, presentado como 004121. |
| Próximo número previsto | 4122, presentado como 004122. |

Se conservará literalmente el formato visual de serie recibido, sin reinterpretar sus grupos. El número 004026 es un ejemplo de presentación de una nota anterior; no reemplaza el último consecutivo 4121 informado por el usuario.

Reglas de emisión:

- Un borrador, una vista previa o una prueba de alineación no consumen números.
- Al confirmar la emisión se guardan juntos la nota y su número. La combinación de serie y consecutivo será única.
- Un doble clic o el reintento de la misma emisión no crearán otra nota.
- El PDF definitivo se genera después de guardar la emisión. Si falla su generación o la impresión, se recupera la misma nota y se conserva el número.
- Regenerar el PDF, cambiar el perfil de impresora o reimprimir no consume numeración.
- Las notas anuladas permanecen en el historial y su número no se reutiliza.
- Las notas emitidas conservarán su contenido; los cambios de datos se harán mientras sean borradores. Si se necesita corregir una emisión, se conservará el registro y se definirá su reemplazo mediante anulación y nueva emisión.
- Se revalidará el último número antes de la puesta en marcha, por si durante el desarrollo se han emitido otras notas.
- Las pruebas usarán datos y numeración separados de los documentos reales.

## 3. PDF e impresión sobre papel preimpreso

La plantilla adjuntada por el usuario se mantendrá como referencia de diseño en todas las etapas. Sus zonas y las reglas de superposición están documentadas en [REFERENCIA_PLANTILLA_NOTA_VENTA.md](REFERENCIA_PLANTILLA_NOTA_VENTA.md), y su uso queda registrado en las instrucciones del proyecto, AGENTS.md.

El formulario mide 20 cm de ancho por 14,5 cm de alto. El PDF tendrá una página de 200 × 145 mm en orientación horizontal.

El programa colocará únicamente:

- Serie y consecutivo en el espacio reservado, siguiendo la referencia recibida.
- Datos del cliente y fecha.
- Cantidad, descripción, precio unitario e importe de cada línea.
- Total en dólares.

El logo, encabezado, autorización, líneas, títulos de columnas, pie y espacios de firmas ya impresos se aprovecharán como parte del papel. No se volverán a imprimir. La serie se podrá ocultar en la salida si un lote de formularios ya la incluye; la configuración inicial parte de la indicación del usuario de que la secuencia no está preimpresa.

La vista previa mostrará el formulario como fondo y los datos superpuestos en sus espacios. «PDF con plantilla» incluirá el documento completo, para revisión o papel blanco. «PDF solo datos» contendrá solo los elementos necesarios para completar el papel preimpreso. Las dos modalidades compartirán las posiciones y ajustes de la vista previa, sin exportar sus guías. La serie que ya contiene la imagen no se duplicará en la salida completa.

### Compatibilidad y calibración

- La Epson L3250 es el modelo inicial de referencia. Se mantiene el objetivo de usar el PDF con otras impresoras, aunque el programa se ejecute primero en una sola computadora.
- La salida principal será PDF, para abrirlo en un lector y seleccionar allí la impresora.
- Se indicará papel personalizado de 200 × 145 mm e impresión en «Tamaño real» o al 100 %, sin ajuste automático de página.
- Cada impresora de uso deberá admitir ese papel, su alimentación y los márgenes necesarios. El PDF por sí solo no garantiza la alineación en todos los modelos.
- Se guardarán perfiles de calibración con desplazamiento horizontal y vertical en milímetros y los ajustes de campos necesarios, sin cambiar el tamaño físico de la página.
- Cambiar de perfil modifica la ubicación de impresión, no los datos ni el número de la nota emitida.
- La alineación se verificará en formularios reales, empezando por la Epson L3250. Las posiciones tomadas de la imagen son una referencia inicial, pendiente de prueba física.
- La prueba de alineación usará datos ficticios y una identificación de prueba, sin consumir números reales.
- Antes de emitir se verificará que los conceptos y sus descripciones caben en los espacios del formulario. Para la primera versión se propone impedir la emisión si el contenido excede la plantilla y explicar qué debe ajustarse; la regla se revisará con el prototipo.

## 4. Etapas y entregables

| Etapa | Trabajo | Entregable | Criterio de aceptación |
|---|---|---|---|
| 1. Definir la nota de venta | Documentar los datos, precios manuales, numeración, PDF y alcance local. | Especificación del primer entregable funcional. | El funcionamiento refleja las prioridades y el formato recibido. |
| 2. Revisar pantallas y plantilla | Preparar el formulario de captura, vista previa y muestra de datos a escala con numeración de prueba. | Prototipo de la nota de venta y PDF de alineación. | Se puede revisar el recorrido y hacer una primera prueba física sin emitir notas reales. |
| 3. Construir la nota de venta local | Implementar captura manual, cálculos, borradores, emisión, guardado, consulta y PDF. | Primer entregable funcional en una computadora. | Una nota puede crearse, emitirse, guardarse y exportarse; reiniciar conserva sus datos y numeración. |
| 4. Validar la impresión | Ajustar perfiles, posiciones y control de textos extensos sobre el papel real. | PDF y configuración calibrada para las impresoras probadas. | Número, cliente, detalle y total calzan en sus espacios al imprimir al 100 %. |
| 5. Completar la entrega local | Terminar anulación, respaldo, restauración, instalador y guía. | Primera versión local lista para uso, con registro de pruebas. | Se completa el recorrido de emitir, imprimir, consultar, reimprimir y recuperar un respaldo en el equipo de uso. |
| 6. Incorporar el cotizador | Crear, guardar y consultar cotizaciones con precios manuales, y convertirlas en notas de venta. | Módulo de cotizaciones integrado. | La conversión conserva cliente, conceptos e importes y solo consume número al emitir la nota. |
| 7. Ampliar a la red Wi-Fi | Migrar la información local y habilitar 2 o 3 computadoras con emisión compartida. | Versión multiusuario en una futura fase. | Se conservan los documentos existentes y las emisiones simultáneas no duplican números. |

Las etapas 1 a 5 describen la primera entrega local. Por la última instrucción del usuario, se adelantó la etapa 7 antes de la 6: la numeración central, emisión e historial están implementados en Bodoni.Red.exe. Su guía es GUIA_RED.md y su revisión está en entregables/REVISION_RED.md. La anulación, restauración asistida y el cotizador continúan pendientes.

## 5. Cómo mediremos el avance

Cada etapa tendrá uno de estos estados: pendiente, en desarrollo, en revisión, con ajustes o aceptada.

Cada entrega incluirá lo disponible, cómo probarlo, los resultados de las comprobaciones y los pendientes encontrados. El progreso se medirá por entregables aceptados dentro de la fase en curso; no se confundirá con tiempo consumido. Las estimaciones se ajustarán al definir el prototipo y comprobar la impresión.

El documento ENTREGABLE_01_ESPECIFICACION.md detalla el alcance y los ejemplos de aceptación de la nota de venta local.

## 6. Comprobaciones de la primera versión

- Una nota con 100 unidades a $0,25 y 2 unidades a $12,50 produce un total de $50,00.
- Cantidades y precios inválidos se detectan antes de emitir.
- Guardar, editar y abrir borradores no modifica el consecutivo.
- Al emitir dos notas sucesivas, partiendo de 4121 en una base de prueba, aparecen 004122 y 004123 con la serie indicada.
- Un doble clic o reintento de la misma emisión no duplica notas ni números.
- Cerrar y abrir el programa conserva notas y consecutivo.
- Regenerar un PDF o reimprimir conserva datos y número, incluso después de un fallo de generación o impresión.
- Anular una nota conserva el registro y no libera su número.
- El PDF mide 200 × 145 mm y no repite elementos que ya están impresos en el papel.
- El PDF completo incluye el logo, textos, líneas y fondo original con los datos superpuestos; el usuario puede distinguirlo de la salida solo datos por su botón y nombre de archivo.
- Los textos y el detalle no invaden otras casillas ni se cortan sin aviso.
- La prueba de alineación no consume numeración.
- La calibración se conserva al reiniciar y cada perfil mantiene el mismo contenido de la nota.
- Se realiza una impresión física en cada impresora que se acepte como calibrada.
- Una copia de seguridad restaura notas, configuración y consecutivo en un entorno de prueba.

## 7. Ampliaciones posteriores

El cotizador reutilizará los datos y cálculos de la nota de venta, agregando su propio estado y presentación. Las cotizaciones tendrán un título diferenciado y podrán incorporar el logo de Bodoni. Su desarrollo se retoma después de validar la conexión de los equipos físicos.

La versión de red ya implementa un servicio y almacenamiento central al que se conectan los 2 o 3 equipos. Los borradores y ajustes del prototipo se conservan; no existían emisiones reales del prototipo para migrar. Todos los equipos emiten a través del principal, sin contadores independientes ni una base abierta desde una carpeta compartida.

Se probaron emisión simultánea con tres clientes TCP, reintentos, cortes de conexión, reinicio y recuperación. Un equipo sin conexión no emite notas por su cuenta. Falta instalar la aplicación en el principal físico elegido por el usuario y comprobar la conexión entre computadores.

Inventario, contabilidad, facturación electrónica, sincronización entre sucursales, cálculo automático de costos y envío de mensajes quedan fuera de la primera versión.

## 8. Pendientes para el prototipo y la puesta en marcha

- Versión de Windows de la computadora inicial.
- Ubicación exacta de los campos y prueba de alimentación del papel en la Epson L3250.
- Revisión de la regla de redondeo y del límite de conceptos con el prototipo.
- Revalidación del último consecutivo al comenzar a operar.
- Destino de la copia de seguridad y procedimiento de recuperación.

La serie ya tiene referencia visual: «001-001-000». El ejemplo «004026» se usa para su presentación, y el siguiente consecutivo previsto sigue siendo «004122».

La versión actual es Bodoni.Red.exe. Su instalación en red se describe en GUIA_RED.md. Los prototipos anteriores se conservan, pero no emiten números reales ni ofrecen sincronización: deben reemplazarse en el uso diario por la versión de red.

La plantilla está incluida y el usuario confirmó la impresión al tamaño correcto. La emisión real y el consecutivo persistente central ya están implementados; los borradores conservan PRUEBA. Al instalar una vez en el otro computador elegido como principal se inicializará con último 004121 y siguiente 004122, salvo que se hayan emitido más notas antes de la puesta en marcha.
