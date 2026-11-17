# Referencia de la plantilla de nota de venta Bodoni

La plantilla compartida por el usuario es la referencia obligatoria del diseño. Los datos se ubicarán sobre sus espacios; se conservará la distribución del formulario en las siguientes etapas del proyecto.

## Fuentes visuales

1. Imagen completa de la nota en blanco, adjuntada al inicio de la conversación: referencia de logo, encabezado, datos del cliente, tabla, total y firmas.
2. Imagen del encabezado de una nota emitida, adjuntada posteriormente: referencia de «SERIE 001-001-000» y del número «004026» en rojo.

La plantilla completa está disponible en [modelo nota e venta.jpg](modelo%20nota%20e%20venta.jpg), de 2247 × 1534 píxeles. El programa la incluye como recurso incrustado `Bodoni.Plantilla.jpg` para mostrarla automáticamente. Una plantilla personalizada guardada en `datos_prototipo/plantilla.png` conserva la preferencia del usuario. El recorte del encabezado sigue como referencia en la conversación.

## Distribución que debe respetarse

| Zona | Elementos de referencia | Datos que se superponen |
|---|---|---|
| Encabezado izquierdo y central | Logo Bodoni, identificación de la empresa y contactos. | Ninguno sobre los elementos ya preimpresos. |
| Encabezado derecho | Recuadro «NOTA DE VENTA», serie, espacio de numeración y autorización. | Consecutivo de seis dígitos; serie si falta en el formulario. |
| Primera línea de cliente | «Sr. (es)» a la izquierda y «Fecha Emisión» a la derecha. | Nombre y fecha. |
| Segunda línea de cliente | «Dirección» a la izquierda y «Teléfono» a la derecha. | Dirección y teléfono. |
| Tercera línea de cliente | «C.I./R.U.C.» a la izquierda y «Mail» a la derecha. | Identificación y correo. |
| Tabla central | Columnas «CANT.», «DESCRIPCION», «V. UNIT.» y «TOTAL». | Cantidad, descripción, precio unitario e importe en cada fila disponible. |
| Parte inferior derecha | Rótulo «TOTAL US$» y casilla del total. | Total de la nota dentro de la casilla. |
| Parte inferior y pie | Firma autorizada, recibido conforme, leyendas y datos impresos del formulario. | Ninguno sobre los elementos ya preimpresos. |

## Vista previa e impresión

- Papel de 200 mm de ancho por 145 mm de alto, horizontal.
- La vista previa mostrará la plantilla como fondo de referencia y los datos superpuestos en su ubicación de impresión.
- El PDF para el formulario preimpreso contendrá únicamente los datos y la secuencia. El fondo de referencia no se incluirá en esa salida.
- El botón «PDF con plantilla» generará una segunda salida con el fondo original completo y los datos superpuestos, para visualizar la nota completa o imprimir sobre papel blanco. Usará la misma imagen de la vista previa, incluida una imagen personalizada si se ha cargado.
- La casilla de serie afecta al PDF solo datos. La vista con fondo y el PDF completo aprovechan la serie que ya aparece en la imagen, sin duplicarla. Ningún PDF incluye las guías de ubicación.
- La vista previa y el PDF usarán una misma definición de posiciones y dimensiones de campos. Los ajustes del perfil de impresora se reflejarán en ambos.
- Las medidas de los campos se expresarán en milímetros. No se asumirá que los píxeles de la imagen equivalen directamente a las medidas del papel.
- Se calibrará sobre una hoja real, empezando por la Epson L3250. Se comprobarán especialmente número, líneas del cliente, primera y última fila utilizada y total.
- Si un texto o la cantidad de conceptos excede su espacio, se avisará antes de emitir. Los datos no deben invadir otras casillas, firmas ni pie.
- Una prueba de alineación se identificará como prueba y no consumirá numeración real.

## Ubicaciones iniciales corregidas

La imagen original disponible está recortada al formulario, mientras que la primera referencia del chat tenía márgenes blancos externos. Se corrigieron las ubicaciones para la imagen de 2247 × 1534 píxeles. Las medidas siguientes son líneas base de texto desde el borde superior del papel, pendientes de calibración física:

| Campo | Línea base (mm) |
|---|---|
| Consecutivo de prueba | 15,7, dentro del recuadro y sobre la autorización. |
| Cliente y fecha | 37,2 |
| Dirección y teléfono | 44,3 |
| Identificación y correo | 51,4 |
| Primera fila del detalle | 64,7, con separación de 6,18 mm entre filas. |
| Total | 131,0 |

La serie ya aparece en la plantilla original y se deja desactivada en la salida de datos por defecto. El logo y todos los textos fijos se ven automáticamente como parte del fondo de la vista previa.

## Numeración de referencia

Serie visible: **001-001-000**. Presentación del consecutivo: seis dígitos con ceros a la izquierda, usando la muestra roja como referencia visual.

El número **004026** pertenece al ejemplo compartido. El último consecutivo informado fue **4121**; el siguiente previsto es **004122**, sujeto a revalidación al poner en marcha el programa.
