# Entregable 1 — Especificación de la nota de venta local

Estado: referencia del alcance local original. La versión actual Bodoni.Red.exe ya incorpora emisión real, numeración central e historial según la nueva prioridad del usuario. Ver GUIA_RED.md y entregables/REVISION_RED.md para instalarla; falta la conexión de los computadores físicos.

## Alcance y prioridad

Primero se desarrolló la nota de venta. El usuario confirmó la impresión al tamaño correcto y adelantó la numeración compartida por red antes del cotizador. El principal será otro computador y el último número confirmado es 4121.

La primera versión guardará la información en la propia computadora y funcionará sin internet ni un equipo servidor. El usuario podrá crear una nota directamente, sin registrar antes una cotización.

| Tema | Definición de la primera versión |
|---|---|
| Plataforma | Aplicación de escritorio para Windows, en una computadora. |
| Documento principal | Nota de venta. |
| Datos del cliente | Nombre, identificación, dirección, teléfono y correo. |
| Fecha | Fecha de emisión mostrada en el documento. |
| Detalle | Cantidad, descripción y precio unitario ingresados manualmente. |
| Cálculos | Importe por línea y total del documento. |
| Papel | Formulario preimpreso de 20 cm de ancho por 14,5 cm de alto. |
| Salida | PDF de 200 × 145 mm en dos modalidades: con plantilla completa o solo datos para papel preimpreso. |
| Impresoras | Epson L3250 como referencia inicial; PDF y perfiles para otras impresoras compatibles con el papel. |
| Diseño físico | Se aprovechan logo, encabezado, líneas y demás elementos preimpresos. |
| Almacenamiento | Borradores, notas, configuración y consecutivo guardados localmente. |
| Consulta | Historial para recuperar notas, regenerar su PDF y reimprimir. |
| Respaldo | Copia y restauración de documentos, configuración y numeración. |

## Serie y consecutivo

La imagen recibida muestra literalmente «SERIE 001-001-000» y, debajo, el número «004026» en rojo. Se usará esta distribución como referencia, con serie y número configurados como elementos separados.

| Dato | Valor previsto |
|---|---|
| Serie visible | 001-001-000 |
| Formato del consecutivo | Seis dígitos, completados con ceros a la izquierda. |
| Número de ejemplo de una nota anterior | 004026; no se utilizará para reiniciar la secuencia. |
| Último consecutivo informado | 4121, mostrado como 004121. |
| Primera emisión prevista | 4122, mostrada como 004122. |

Antes de operar se comprobará que 4121 siga siendo el último número utilizado. Las pruebas de desarrollo usarán una base separada de los documentos reales.

En el PDF solo datos, la serie se imprimirá cuando no esté preimpresa; se podrá ocultar para un formulario que ya la incluya. Esa salida no repetirá el título «NOTA DE VENTA» ni la autorización. El PDF con plantilla incluye todos los elementos gráficos originales y aprovecha su serie ya impresa, sin duplicarla con una segunda capa de texto.

## Flujo de uso

1. Seleccionar «Nueva nota de venta».
2. Completar cliente, fecha y conceptos con precios manuales.
3. Revisar los importes y el espacio disponible en el formulario. Guardar como borrador si todavía no debe emitirse.
4. Confirmar la emisión: guardar juntos el documento definitivo y su siguiente número.
5. Elegir el perfil de impresora y la salida: PDF con plantilla para documento completo o PDF solo datos para formulario preimpreso.
6. Abrir el PDF e imprimir al 100 %, sin ajuste automático: papel blanco para el completo o formulario preimpreso para solo datos.
7. Consultar la nota en el historial para regenerar su PDF o reimprimir con el mismo número.

Un borrador o prueba de alineación no consumirá números. Un doble clic o reintento no creará una segunda emisión. Si falla el PDF o la impresión después de emitir, se recuperará la nota existente. Las anulaciones conservarán el número y el registro; los números anulados no se reutilizarán.

Las notas emitidas conservarán su contenido. La edición se realizará en borradores; una corrección posterior requerirá conservar el registro y gestionar anulación y nueva emisión.

## Reglas de cálculo propuestas para revisar

- Moneda: dólares, con importes a dos decimales.
- Cantidades mayores que cero y precios no negativos; cada concepto tendrá descripción.
- Importe por línea: cantidad × precio unitario, redondeado a centavos.
- Total: suma de los importes de las líneas.
- Cálculo decimal con redondeo de mitades hacia arriba, pendiente de validación con el prototipo.
- Sin cálculo automático de materiales, medidas, acabados ni margen de ganancia en esta entrega.
- Sin descuentos o impuestos agregados automáticamente; cualquier incorporación requerirá una regla acordada.

Ejemplo de aceptación:

| Cantidad | Descripción de prueba | Precio unitario | Importe |
|---|---|---|---|
| 100 | Trabajo A | $0,25 | $25,00 |
| 2 | Trabajo B | $12,50 | $25,00 |
| | **Total esperado** | | **$50,00** |

## PDF y alineación

La plantilla compartida por el usuario es la referencia obligatoria y permanente del documento. Su distribución se registra en [REFERENCIA_PLANTILLA_NOTA_VENTA.md](REFERENCIA_PLANTILLA_NOTA_VENTA.md). Los datos se superpondrán en los espacios de esa plantilla, conservando sus columnas, filas y casillas.

Ambas modalidades de PDF tendrán una página horizontal de 200 × 145 mm. «PDF solo datos» contendrá los datos para completar el formulario: serie cuando corresponda, consecutivo, fecha, cliente, detalle y total. «PDF con plantilla» incorporará además el fondo original completo.

La vista previa mostrará la plantilla como fondo con los datos superpuestos. El PDF completo usará ese mismo fondo y conservará el texto variable como texto seleccionable sobre la imagen. El logo y los elementos gráficos se omitirán solo en el PDF de datos. Las posiciones se ajustarán con una prueba sobre el papel real. Vista previa y ambas salidas compartirán una misma definición de posiciones.

Se guardarán perfiles por impresora con desplazamientos horizontal y vertical en milímetros y los ajustes de campos necesarios. Un cambio de perfil no alterará la serie, número, contenido ni tamaño físico de página.

Para imprimir el PDF, la impresora deberá admitir el papel personalizado de 200 × 145 mm y los márgenes necesarios. Se comprobará primero la Epson L3250. Los demás modelos se aceptarán conforme se prueben y calibren; el uso inicial de una computadora no impide seleccionar varias impresoras desde el lector de PDF.

Las pruebas de alineación tendrán datos ficticios e identificación de prueba y no consumirán numeración. Si un concepto o el conjunto de líneas excede el espacio, se propone bloquear la emisión y pedir un ajuste antes de asignar un número; se revisará con el prototipo.

## Criterios para aceptar el entregable funcional

| Comprobación | Resultado esperado |
|---|---|
| Captura manual | Se puede crear una nota sin cotización previa y obtener $50,00 en el ejemplo. |
| Borrador | Guardar, cerrar, abrir y editar no consume números. |
| Emisión | En una base de prueba con último número 4121, dos emisiones producen 004122 y 004123 con serie 001-001-000. |
| Doble clic o reintento | Una misma emisión produce una sola nota y un solo número. |
| Persistencia | Cerrar y abrir el programa conserva documentos y consecutivo. |
| PDF | Página de 200 × 145 mm con datos y secuencia, sin repetir el diseño preimpreso. |
| PDF completo | Página de 200 × 145 mm con la imagen original de fondo y los datos visibles encima, sin duplicar la serie ni incluir guías. |
| Fidelidad a la plantilla | La vista previa usa el formulario de Bodoni como referencia y los datos coinciden con sus espacios; la distribución no se reemplaza por otra. |
| Reimpresión | Regenerar el PDF, cambiar de perfil o reimprimir conserva número y contenido. |
| Fallo de PDF o impresora | Se recupera la nota emitida y se vuelve a intentar sin consumir otro número. |
| Anulación | La nota permanece identificada en el historial y su número no se reutiliza. |
| Alineación | Serie, número, cliente, detalle y total calzan en los espacios del formulario físico probado. |
| Respaldo | Una copia restaura documentos, configuración y consecutivo en un entorno de prueba. |

El prototipo y PDF de prueba se revisarán antes de la versión funcional. La calibración física, el respaldo y el instalador completarán la entrega local según las etapas del plan.

El prototipo actualizado se abre con Bodoni.Prototipo.Actualizado.exe. La plantilla original con su logo y textos fijos ya está incluida y aparece automáticamente junto con datos de ejemplo o el último borrador recuperado. Sus instrucciones y límites están en README.md; la lista de revisión está en entregables/REVISION_ENTREGA_01.md. Los PDF del prototipo muestran PRUEBA y no corresponden a documentos emitidos.

## Fuera de esta primera fase

- Cotizaciones y conversión de cotización a nota: siguiente módulo, después de validar la nota de venta.
- La conexión entre computadoras fue adelantada e implementada después de la primera fase; su instalación física se detalla en GUIA_RED.md.
- Cálculo automático de costos, inventario, contabilidad y facturación electrónica.

## Pendientes para las siguientes revisiones

- Identificar la versión de Windows del equipo inicial.
- Verificar las posiciones de campos y la alimentación del papel en la prueba física.
- Revisar el redondeo y los límites de texto y conceptos con el prototipo.
- Elegir el destino de las copias de seguridad antes de la puesta en marcha.
- Comprobar el último número real al activar la aplicación.

No se necesita elegir un equipo servidor para esta entrega. El diseño e instalación de la red se definirán cuando se aborde esa fase.
