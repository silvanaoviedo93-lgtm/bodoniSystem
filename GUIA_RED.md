# Bodoni — Consecutivo compartido entre computadoras

Esta versión usa **un computador principal** para guardar las notas emitidas y asignar sus números. Los otros 1 o 2 equipos se conectan a ese mismo principal. El principal también puede emitir notas.

El usuario confirmó que el principal será **otro computador** y que el último número utilizado es **004121**. La primera emisión de una instalación nueva recibirá **004122**, con la serie **001-001-000**.

## 1. Preparar el computador principal

1. Lleva **Bodoni-Red.zip** al computador que permanecerá encendido.
2. Usa **Extraer todo** y guarda la carpeta en una ubicación fija local, por ejemplo Documentos\Bodoni. No ejecutes el programa directamente desde el ZIP ni desde una carpeta compartida.
3. Abre **Bodoni.Red.exe**.
4. Pulsa **Red y numeración** y selecciona **Este computador es el principal**.
5. Deja el puerto **47831** y verifica **Último número utilizado: 4121**. Si ya se han emitido más notas al instalar, coloca el último real antes de iniciar por primera vez.
6. Pulsa **Guardar y conectar**. Debe aparecer **PRINCIPAL · Último 004121 · Próximo 004122**. Configurar o consultar la conexión no consume números.
7. Mantén el programa abierto y el computador encendido mientras los demás trabajen. Si el equipo se suspende o se cierra Bodoni, los demás no podrán emitir hasta recuperar la conexión.

En una instalación existente se conserva el consecutivo guardado: cambiar el campo inicial no lo reinicia. Si faltan o están dañados los datos de un principal ya inicializado, la aplicación detiene la emisión en lugar de volver a 4121.

## 2. Permitir la conexión en Windows

Solo en el principal:

1. La conexión de tu empresa debe estar marcada como **red privada** en Windows.
2. En la carpeta del programa, haz clic derecho sobre **Habilitar_Red.cmd** y selecciona **Ejecutar como administrador**.
3. El archivo preparará la entrada TCP por el puerto 47831 para Bodoni, limitada a la red privada y la subred local. No se necesita abrir puertos en el router ni configurar internet.

Este paso se ejecutará en el principal que elegiste; no se ha cambiado el firewall de este computador. Si mueves el programa a otra carpeta, vuelve a ejecutar el archivo en su nueva ubicación. Si eliges un puerto diferente, ejecuta el script PowerShell con ese puerto o vuelve al 47831.

La regla utiliza los parámetros de [New-NetFirewallRule de Microsoft](https://learn.microsoft.com/en-us/powershell/module/netsecurity/new-netfirewallrule?view=windowsdefender).

## 3. Exportar la conexión del principal

1. En el programa del principal, pulsa **Exportar conexión**.
2. Guarda **Conexion-Bodoni.bodoni-red.json**.
3. Lleva ese archivo a los equipos secundarios, junto con el ZIP del programa.

El archivo contiene la identificación y clave para conectarse a tu principal. Entrégalo únicamente a los equipos autorizados para emitir y consultar notas de Bodoni. No contiene una copia del contador para que cada equipo lo utilice por separado.

## 4. Conectar este computador y los otros secundarios

1. Extrae el ZIP a una carpeta local fija y abre **Bodoni.Red.exe**. En este computador también puedes usar el ejecutable que ya está en la carpeta Cotizador Bodoni.
2. Pulsa **Red y numeración** y deja **Otro computador es el principal**.
3. Pulsa **Importar conexión del principal** y selecciona **Conexion-Bodoni.bodoni-red.json**.
4. Pulsa **Guardar y conectar**.
5. Comprueba que aparezcan el último número y el próximo, consultados al mismo principal. La pantalla se actualiza aproximadamente cada cuatro segundos.

Si el nombre del principal no conecta, reemplázalo por su dirección IPv4 dentro de **Red y numeración**. Puedes verla en el principal abriendo Símbolo del sistema y escribiendo `ipconfig`; toma la IPv4 del adaptador conectado a la red de la empresa. La dirección de este computador no debe usarse como principal, porque elegiste otro equipo.

Los equipos deben poder comunicarse dentro de la misma red local. Una red Wi-Fi de invitados con aislamiento entre dispositivos no sirve para esta conexión. Si una IP cambia, corrígela en los clientes o configura una reserva en tu router con ayuda de quien administra la red.

## 5. Emitir una venta con número real

1. Pulsa **Nueva** y completa el cliente, fecha y conceptos.
2. Revisa importes y distribución. Los PDF de un borrador siguen mostrando **PRUEBA** y no consumen un número.
3. Pulsa **Emitir y asignar número** y confirma la emisión de esa venta real.
4. El principal guarda juntos la nota y el siguiente consecutivo. La primera será **004122** si no hubo otra emisión anterior.
5. La pantalla mostrará **NOTA EMITIDA 004122** y bloqueará la edición del documento.
6. Genera **PDF solo datos** para tu papel preimpreso o **PDF con plantilla** para ver el documento completo o imprimir en papel blanco.
7. Imprime con la configuración que ya probaste: **200 × 145 mm**, tamaño real al **100 %**.

Dos equipos pueden mostrar el mismo «Próximo» antes de emitir: es una consulta del estado, no una reserva. Al confirmar, el principal asigna números distintos según el orden de guardado.

La numeración real se asigna al **emitir**, no al abrir el programa ni al exportar un PDF. Exportar ambas modalidades o reimprimir conserva el mismo número.

## 6. Si se corta la conexión durante una emisión

La aplicación conserva localmente la solicitud antes de enviarla. Si no llega la respuesta, mostrará **EMISIÓN PENDIENTE** y el botón **Recuperar emisión**.

1. Recupera la conexión con el principal.
2. Pulsa **Recuperar emisión**.
3. Si el principal ya guardó la nota, devolverá ese mismo número. Si aún no la guardó, completará esa misma solicitud con el siguiente disponible.

No se habilita otra emisión para sustituir una pendiente. Cerrar y volver a abrir conserva la solicitud. Ningún secundario inventa un número cuando no puede contactar al principal.

## 7. Consultar y reimprimir

Pulsa **Historial** para consultar las últimas 200 notas emitidas en el principal, incluidas las emitidas desde otros equipos. Selecciona una y pulsa **Abrir para consultar o reimprimir**. Sus PDF conservarán el número original.

Para otra venta, pulsa **Nueva**. La edición de notas emitidas, anulación y herramientas de restauración quedan para una entrega posterior; en esta versión las notas guardadas se conservan sin cambios y sus números no se reutilizan.

## 8. Dónde se conservan los datos

- **Principal:** conserva la carpeta **datos_bodoni** junto al programa. Dentro de **principal** están el registro de notas, el contador y su marca de inicialización. **conexion-red.json** identifica ese principal.
- **Secundarios:** conservan en **datos_bodoni** su conexión, borrador, solicitud pendiente y última nota consultada. No tienen un contador independiente para emitir.
- **Impresión:** cada equipo conserva sus propios perfiles y plantilla personalizada. Si usas la misma carpeta del prototipo anterior, se copian sus ajustes y último borrador a la nueva carpeta de datos sin borrar los originales.

Para una copia manual, cierra Bodoni en el principal cuando no haya emisiones en curso y copia **toda la carpeta datos_bodoni** a otro destino. No borres ni reemplaces el registro central para reiniciar el programa, y no restaures una copia antigua para continuar la numeración sin revisar qué notas se emitieron después.

Configura **un solo principal**. Copiar el ejecutable a un secundario no lo convierte automáticamente en principal. Los secundarios importan la conexión del equipo elegido; no deben trabajar con una copia independiente de los datos centrales.

## 9. Validación de esta entrega

Se probaron tres clientes TCP simultáneos, 30 emisiones consecutivas, reintentos de la misma solicitud, pérdida de respuesta, reinicio del principal, falta de conexión, errores de guardado y recuperación de notas para PDF. Los resultados están en **entregables/VERIFICACION_RED.txt**. Los registros de estas pruebas se aislaron y no consumieron números reales.

La impresión al tamaño correcto fue confirmada por el usuario. Falta instalar este paquete en el principal físico elegido y comprobar la conexión desde los otros equipos. Para esa primera comprobación basta con ver el mismo último número en ambos, sin emitir ventas de prueba con números reales.

El transporte usa los componentes [TcpClient y TcpListener de .NET](https://learn.microsoft.com/en-us/dotnet/framework/network-programming/using-tcp-services). Esta entrega está pensada para la red local de la empresa.
