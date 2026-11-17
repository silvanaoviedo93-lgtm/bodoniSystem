# Reglas del proyecto Bodoni

Antes de diseñar o modificar la nota de venta, leer PLAN_DESARROLLO.md, ENTREGABLE_01_ESPECIFICACION.md y REFERENCIA_PLANTILLA_NOTA_VENTA.md. Las instrucciones posteriores del usuario tienen prioridad.

- La plantilla de Bodoni adjuntada por el usuario es la referencia del documento. Respetar su distribución, casillas, columnas, encabezado, logo y espacios. No sustituirla por un diseño genérico.
- Mostrar la plantilla como fondo de referencia en la vista previa y superponer los datos en sus espacios.
- El papel ya contiene el diseño. El PDF para imprimir sobre ese papel debe contener únicamente datos variables y la secuencia, sin volver a imprimir logo, bordes, rótulos ni otros elementos preimpresos. Imprimir la serie solo cuando corresponda al formulario utilizado.
- También ofrecer «PDF con plantilla»: incluye el fondo original, logo, textos, líneas y datos, para revisión completa o impresión sobre papel blanco. Mantener separado «PDF solo datos» para papel preimpreso; los botones y nombres de archivo deben distinguir ambos destinos.
- El PDF con plantilla debe usar la misma imagen de fondo cargada en la vista previa. La serie ya presente en esa imagen no se vuelve a superponer; la opción PrintSeries corresponde únicamente al PDF solo datos. Las guías de campos no se exportan en ninguna modalidad.
- Usar una página de 200 × 145 mm. Vista previa y PDF deben compartir las posiciones de los campos; calibrarlas en milímetros y validar con una impresión física al 100 %.
- La plantilla original ya está disponible en `modelo nota e venta.jpg` (2247 × 1534 píxeles) y se incrusta sin modificar en el ejecutable como `Bodoni.Plantilla.jpg`. La versión de red usa `datos_bodoni/plantilla.png` si hay una personalizada e importa los ajustes de `datos_prototipo` sin borrar los originales. No sustituir la plantilla por una recreación.
- El formulario original ya incluye la serie: dejar `PrintSeries` desactivado por defecto para evitar duplicarla. El número variable ocupa el espacio libre debajo de la serie y encima de la autorización.
- La versión actual usa `datos_bodoni/tramite-actual.json` para conservar borradores, solicitudes pendientes y notas emitidas consultadas; importa el borrador anterior si no existe. Si hay una emisión pendiente, conservar su identificador y bloquear su sustitución hasta recuperarla. Nunca volver a emitirla con otro identificador por una pérdida de conexión.
- El usuario adelantó la sincronización por red antes del cotizador. La impresión al tamaño correcto ya fue confirmada. El principal será OTRO computador, no el de esta sesión; último número confirmado 4121, siguiente previsto 004122.
- Usar un único principal y un registro central con guardado conjunto y durable de nota y contador, autenticación por conexión e identificación del servidor. Los secundarios no asignan números por su cuenta. Un registro ausente o dañado de un principal ya inicializado debe bloquear la emisión, nunca reiniciar la secuencia.
- Las pruebas de red deben usar datos aislados y un servidor TCP local de prueba, sin tocar registros de uso. No afirmar que se probaron los computadores físicos cuando solo se probaron clientes locales.
- Serie visual de referencia: 001-001-000. El ejemplo 004026 no es el último consecutivo. Se informó 4121 como último utilizado; el siguiente previsto se muestra como 004122. Revalidarlo antes del uso real.
- Borradores y pruebas no consumen números; reimpresiones y regeneraciones de PDF conservan el número de la nota emitida.
