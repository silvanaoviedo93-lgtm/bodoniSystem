# Revisión — Prototipo de nota de venta

Estado: disponible para revisión del usuario. No es todavía la versión para emitir notas reales.

## Entregado

- Ejecutable de Windows con captura manual y cálculos.
- Guardado y recuperación de borradores de prueba.
- Plantilla original completa incluida dentro del programa, con logo y textos fijos, y recuperación automática del último borrador.
- PDF de 200 × 145 mm en dos modalidades: con plantilla completa y solo datos para papel preimpreso. Ambas mantienen la identificación PRUEBA.
- Perfiles de impresión con desplazamientos, tamaño de letra y opción de serie preimpresa.
- Muestras PDF, borrador de $50,00, captura de pantalla y guía de uso.

## Verificado

El registro VERIFICACION.txt contiene las comprobaciones automáticas y sus resultados: decimales, redondeo, datos inválidos, ancho de textos, diez filas, tamaño PDF, fuentes incrustadas, persistencia de borradores y perfiles, edición del detalle y validación de cliente.

El registro REVISION_PANTALLA.txt corresponde a la apertura breve de la ventana y captura del prototipo. Se revisó visualmente la captura y se comprobó la edición de una celda y el cambio del total.

Los cuatro PDF de muestra, completos y solo datos con dos o diez filas, fueron procesados correctamente por el motor Ghostscript instalado en el equipo, sin errores reportados de lectura o fuentes. Se revisaron visualmente las páginas completas renderizadas. Esta comprobación no valida la alineación física de una impresora.

## Revisión del usuario

- [ ] Abrir Bodoni.Prototipo.Actualizado.exe.
- [ ] Comprobar que plantilla y texto aparecen automáticamente al abrir.
- [ ] Cargar «Ejemplo $50» y confirmar el total.
- [ ] Cambiar cantidades y precios y revisar el cálculo.
- [ ] Guardar, cerrar y recuperar un borrador.
- [ ] Generar y abrir PDF con plantilla y comprobar el logo, líneas y textos fijos.
- [ ] Generar y abrir PDF solo datos para imprimir en el formulario preimpreso.
- [ ] Imprimir al 100 % sobre papel de 200 × 145 mm.
- [ ] Informar qué campos deben desplazarse y cuántos milímetros.

La imagen original ya está disponible en modelo nota e venta.jpg y está incrustada dentro del programa. Se corrigieron las líneas de datos del cliente, los anchos de columnas, la separación de filas y el espacio del número para ajustarlos al formulario recortado. La serie preimpresa se deja desactivada en la salida de datos para evitar duplicaciones. La calibración física en impresora sigue pendiente.

## Siguiente paso

Resolver las observaciones de pantalla y alineación; después implementar la emisión real y persistencia del consecutivo según el plan. El siguiente número previsto permanece en 004122 y se revalidará al comenzar la operación real.
