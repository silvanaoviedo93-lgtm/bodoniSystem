# Bodoni — Notas de venta y numeración compartida

La versión actual es **Bodoni.Red.exe**. Permite emitir notas con un único consecutivo desde 2 o 3 computadores conectados a un principal, consultar el historial compartido y generar PDF completos o solo datos. No requiere internet, pero sí conexión al principal para emitir.

**La instalación se explica en [GUIA_RED.md](GUIA_RED.md).** El principal será otro computador, como indicó el usuario. El último número confirmado es **004121** y el siguiente previsto es **004122**. La instalación y prueba entre computadores físicos sigue pendiente.

Un borrador muestra **PRUEBA** y no consume número. Al pulsar **Emitir y asignar número** y confirmar una venta, el principal guarda la nota y asigna su número real. Generar otro PDF o reimprimir conserva ese número.

## Abrir y probar

1. Abre **Bodoni.Red.exe** desde una carpeta local fija. Sigue GUIA_RED.md para configurar un solo principal y conectar los secundarios. Los prototipos anteriores se conservan pero no sincronizan números.
2. La plantilla de Bodoni, con su logo y todos los textos fijos, aparece automáticamente junto con datos ficticios de $50 o el último borrador recuperado. Puedes pulsar **Nueva** para empezar en blanco o **Ejemplo $50** para volver a la muestra.
3. Ingresa cantidad, descripción y precio unitario. Puedes utilizar coma o punto decimal, sin separadores de miles. Los importes se calculan al terminar de editar la celda.
4. La plantilla original ya viene dentro del programa; no necesitas seleccionarla. **Cambiar plantilla** permite cargar otro archivo completo si luego cambias de formulario. No uses el recorte del encabezado: se necesita el formulario completo, sin márgenes externos ni inclinación.
5. Revisa los datos sobre la imagen. Puedes activar o desactivar las guías de campos; las guías tampoco aparecen en el PDF.
6. Pulsa **PDF con plantilla** para guardar el documento completo con logo, encabezado, líneas y datos. Utiliza **PDF solo datos** cuando vayas a imprimir en las hojas que ya tienen ese diseño.

Se incorporó sin modificar el archivo original **modelo nota e venta.jpg** dentro del ejecutable. Si anteriormente cargaste una plantilla personalizada, se seguirá usando esa copia. No hace falta llevar la imagen por separado para ver el formulario en otro equipo.

## Probar la impresión

Para ver el documento completo puedes abrir **entregables/Bodoni-PRUEBA-completa.pdf**. Si deseas imprimirlo completo, utiliza papel blanco del tamaño indicado.

Para la prueba sobre tus hojas preimpresas usa **entregables/Bodoni-PRUEBA-alineacion.pdf**, que contiene solo los datos. Que se vea sin logo ni líneas en el lector de PDF es el resultado esperado de esa modalidad: el diseño ya está en el papel.

Ambas muestras tienen fecha de prueba 12/11/2026 y total de $50,00; las notas nuevas toman la fecha del equipo. Las siguientes instrucciones de alineación corresponden a la modalidad solo datos:

1. Abre el PDF en tu lector habitual y selecciona la Epson L3250 o la impresora que probarás.
2. Configura el papel a **200 mm de ancho × 145 mm de alto** y revisa la orientación horizontal y la forma de alimentar la hoja.
3. Selecciona **Tamaño real** o **100 %**, sin ajustar a página. La impresora debe admitir ese papel y los márgenes necesarios.
4. Imprime una prueba y compárala con el formulario. Para evitar gastar un formulario puedes empezar en una hoja de prueba de las mismas medidas y superponerla al formulario a contraluz.
5. Si todos los campos están desplazados, corrige **Horizontal** y **Vertical** en el programa. Valores positivos mueven a la derecha y hacia abajo; valores negativos, a la izquierda y hacia arriba.
6. Vuelve a generar el PDF después de modificar el perfil. El PDF anterior conserva sus posiciones originales.

La casilla **Imprimir serie en PDF solo datos** debe desmarcarse si la serie ya aparece en ese lote de papel. El PDF completo aprovecha la serie visible en el fondo y no la duplica. El ajuste de letra permite un tamaño entre 80 % y 120 % del inicial. Cada perfil conserva sus propios ajustes y puede seleccionarse al exportar.

Si unas zonas calzan y otras no, registra qué campo se desplaza y cuántos milímetros. Ese resultado servirá para ajustar las posiciones individuales de la plantilla en la siguiente revisión. Las posiciones actuales son iniciales, tomadas de la referencia visual, y aún no han sido calibradas físicamente.

## Lo que puedes revisar ahora

- Cliente, identificación, dirección, teléfono, correo y fecha.
- Hasta diez conceptos, con cantidad, descripción y precio manuales.
- Cantidades de hasta tres decimales y precios de hasta cuatro; el precio impreso conserva los decimales necesarios. Importes y total se redondean a dos decimales.
- Suma por línea con cálculo decimal y redondeo de mitades hacia arriba.
- Guardado y apertura de borradores, incluso incompletos, en archivos **.bodoni.json**.
- Aviso antes de descartar cambios sin guardar.
- Detección de campos vacíos necesarios para el PDF, números inválidos y textos que exceden su casilla.
- PDF con fuentes incrustadas y tamaño fijo: completo con el mismo fondo de la vista previa, o solo datos sin fondo. Ninguno incluye guías.
- Carga de la plantilla original y perfiles de alineación guardados entre aperturas.

El borrador o trámite actual se recupera automáticamente desde **datos_bodoni/tramite-actual.json**. Si una emisión quedó sin respuesta, conserva su solicitud y permite recuperarla con el mismo número. Puedes usar **Guardar borrador** para archivos con nombres propios. Los perfiles y la plantilla personalizada se conservan; se importan los ajustes anteriores de datos_prototipo sin borrar sus originales.

## Archivos de la entrega

| Archivo | Uso |
|---|---|
| Bodoni.Red.exe | Aplicación actual con plantilla, numeración central y recuperación de emisiones. |
| entregables/Bodoni-Red.zip | Paquete para llevar al otro computador principal y a los secundarios. |
| GUIA_RED.md | Instalación, conexión, emisión y conservación de datos. |
| entregables/VERIFICACION_RED.txt | Pruebas del servicio y tres clientes TCP. |
| modelo nota e venta.jpg | Archivo original de referencia, incluido en el ejecutable. |
| entregables/Bodoni-PRUEBA-completa.pdf | PDF completo con plantilla, logo y datos de ejemplo, total $50,00. |
| entregables/Bodoni-PRUEBA-completa-diez-filas.pdf | PDF completo de prueba con las diez filas utilizadas. |
| entregables/Bodoni-PRUEBA-alineacion.pdf | Muestra de dos líneas, total $50,00. |
| entregables/Bodoni-PRUEBA-diez-filas.pdf | Muestra que ocupa las diez filas del formulario. |
| entregables/ejemplo-50-dolares.bodoni.json | Borrador que puedes abrir desde la aplicación. |
| entregables/prototipo-pantalla.png | Captura del prototipo actualizado. |
| entregables/plantilla-con-texto-prueba.png | Vista del formulario original completo con datos superpuestos. |
| entregables/VERIFICACION.txt | Resultados de las comprobaciones automáticas. |
| entregables/REVISION_PANTALLA.txt | Resultado de la comprobación con ventana visible. |
| datos_bodoni/ | Datos de uso: conexión, trámite, perfiles y, solo en el principal, notas y contador central. |
| datos_prototipo/ | Datos del prototipo anterior, conservados para importar impresión y borradores. |
| src/ | Código fuente del prototipo. |

## Pendientes para las siguientes entregas

La emisión real, numeración central y el historial ya están implementados y probados en TCP local. Falta instalarlos y comprobar la conexión de los computadores físicos. Anulación, restauración asistida, instalador y cotizador siguen pendientes. Las notas emitidas son inmutables; la guía explica cómo conservar sus datos y hacer una copia manual del principal.

Para revisar esta entrega, comprueba la comodidad de la captura, el resultado de $50,00 y la posición de los campos sobre tu formulario. La aceptación de la alineación requiere tu prueba física; no se da por realizada con las comprobaciones de software.

## Desarrollo y comprobaciones

El ejecutable ya está compilado. El código usa C# y los controles de [Windows Forms de Microsoft](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/); el generador de PDF sigue la estructura de objetos y fuentes descrita en la [referencia PDF de Adobe](https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/pdfreference1.3.pdf).

Compilar desde PowerShell, sin instalar dependencias:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Compilar.ps1
```

La opción se aplica únicamente a ese proceso; no cambia la política permanente de Windows. El script utiliza el compilador de .NET Framework disponible en Windows. Si se distribuye a otro equipo, habrá que verificar allí su versión de .NET Framework.

Ejecutar comprobaciones de cálculo, almacenamiento, PDF y controles, sin abrir la ventana ni modificar los datos de uso:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Verificar.ps1
```

Los datos de ejemplo son ficticios. No se ha configurado ni modificado el contador real de notas de venta.

Comprobaciones de red con tres clientes locales y datos aislados:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Verificar_Red.ps1
```
