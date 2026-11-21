# Bodoni — Consecutivo compartido entre computadoras

Esta versión usa **un computador principal** para guardar las notas emitidas y asignar sus números. Los otros 1 o 2 equipos se conectan a ese mismo principal. El principal también puede emitir notas.

El principal de producción previsto tiene **Windows 11**. El último número real aún debe confirmarse. **004121** es la base de práctica, siguiente **004122**, serie **001-001-000**. Todos los equipos deben usar esta misma versión actualizada (protocolo 4, registro 4 con migración de versiones anteriores); no abrir un registro actualizado con ejecutables anteriores.

## Vigencia de autorización y pie de imprenta

**Rango y últimos diez:** el pie original autoriza **004026–004225**. El programa extrae ese rango del texto `# 4026-4225`, muestra cuántos números quedan y alerta desde próximo **004216** (10 disponibles, contando el siguiente). Con próximo 004215 quedan 11. Después de emitir 004225, bloquea nuevas notas hasta registrar una autorización y rango compatibles. El control está en el principal y también se muestra en los secundarios, en la barra y en Última numeración. El aviso permanece visible y aparece al abrir o al cambiar el número disponible mientras Bodoni está abierto.

Al renovar, incluye el nuevo `# inicio-fin` en el pie completo junto con las fechas. La pantalla muestra el rango extraído. No se reinicia ni se salta el contador: si el próximo número queda fuera del nuevo rango, la emisión sigue bloqueada. Anular la última del rango permite reemplazar ese mismo número según la regla acordada, sin habilitar uno fuera del rango. Recuperar una nota ya emitida sigue permitido aun al agotarse los números.

La barra muestra permanentemente el estado de la autorización consultada al principal. Los datos iniciales corresponden a las imágenes recibidas: **AUT. S.R.I. 1133313444**, emisión del pie **09/03/2026**, caducidad **09/03/2027**. No confundir ese número con la **Aut. 13485** de la imprenta.

- Desde **23/02/2027**, avisa que quedan 14 días o menos. Muestra un mensaje al abrir o al cambiar el día mientras Bodoni permanezca abierto, además del estado visible. No genera notificaciones con el programa cerrado.
- El **09/03/2027** muestra «caduca HOY» y todavía permite emitir.
- Desde **10/03/2027**, bloquea Emitir e informa que la autorización está caducada. Poner una fecha anterior en la nota no evita el bloqueo.
- La fecha de cada nota debe quedar entre emisión y caducidad del pie. El principal vuelve a comprobarlo antes de guardar y asignar número. El control usa la fecha del principal en Ecuador continental, no la del secundario.

Después de gestionar una renovación, abre **Pie de imprenta** en el principal. Ingresa el nuevo número AUT. S.R.I. y copia el texto completo del pie, con **Emisión: día/mes/año** y **Caduca: día/mes/año** (mes numérico o en español). Las fechas se extraen al escribir; revisa los campos y pulsa **Guardar pie de imprenta**. Si no coinciden con el texto, el guardado se bloquea. Los secundarios consultan la actualización automáticamente y tienen esta pantalla en modo consulta.

Se conserva el historial del pie anterior y cada nueva nota guarda una copia de su autorización. Renovar no reinicia el consecutivo ni cambia emitidas. Recuperar una solicitud ya emitida sigue permitido aunque su autorización haya caducado. Si el principal confirma que una solicitud no se emitió por autorización, se devuelve a borrador para corregirla; si se pierde la conexión, se mantiene pendiente.

Guardar el pie no modifica la imagen de fondo. Usa **Cambiar plantilla** en cada equipo para cargar la imagen del formulario renovado; el papel preimpreso debe corresponder a esa autorización. El PDF solo datos no vuelve a imprimir el pie ni la autorización. La configuración de práctica conserva estos mismos controles en sus datos separados.

## Practicar desde un solo computador

Abre **Abrir_Practica.cmd**. El encabezado indicará PRÁCTICA y se creará `datos_practica`, separado de los datos normales. Inicia un principal local en el puerto 47832, accesible solo desde este equipo. No requiere red de oficina ni cambios del firewall. La primera emisión de práctica será 004122; reabrir conserva el progreso y las pendientes. Sus PDF muestran PRUEBA junto al número. Esta numeración no se traslada a producción.

Para producción, cierra la práctica y abre **Bodoni.Red.exe** normalmente. En **Red y numeración** configura principal o secundario. Antes de inicializar por primera vez el principal, ingresa el último número real. No copies `datos_practica` a `datos_bodoni` ni cambies el contador de un principal ya inicializado.

## Consultar la numeración y reemplazar la última nota

**Última numeración** consulta el principal y muestra último número utilizado y siguiente disponible, sin consumir ni reservar números. Si la última está anulada, el siguiente disponible es su mismo número.

Abre la nota desde **Historial**, pulsa **Anular última nota**, escribe el motivo y confirma. Se conserva la original con fecha y motivo. Pulsa **Nueva**, prepara el documento corregido y emítelo: si sigue disponible, recibirá ese número. Si otro equipo ya lo reemplazó, se asignará el siguiente disponible. No se puede liberar el número de una nota con otra vigente posterior.

El historial conserva la original y todos sus reemplazos. Repetir la anulación de la original no anula su reemplazo. Si se pierde la respuesta, consulta Historial o reintenta sobre la misma nota. Exportar una emitida requiere consultar su vigencia en el principal y bloquea las anuladas. Los PDF o impresos anteriores quedan fuera del control del programa: retíralos del uso operativo al reemplazarlos.

## 1. Preparar el computador principal

1. Lleva **Bodoni-Red.zip** al computador que permanecerá encendido.
2. Usa **Extraer todo** y guarda la carpeta en una ubicación fija local, por ejemplo Documentos\Bodoni. No ejecutes el programa directamente desde el ZIP ni desde una carpeta compartida.
3. Abre **Bodoni.Red.exe**.
4. Pulsa **Red y numeración** y selecciona **Este computador es el principal**.
5. Deja el puerto **47831** e ingresa el **último número real utilizado**, confirmado en la oficina. El valor 4121 es únicamente una referencia de práctica.
6. Pulsa **Guardar y conectar**. Debe aparecer **PRINCIPAL**, con el último ingresado y su siguiente. Configurar o consultar no consume números.
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

Para otra venta, pulsa **Nueva**. Las notas emitidas conservan su contenido; la corrección de la última se realiza con anulación y reemplazo según la sección anterior. Las herramientas de restauración siguen pendientes.

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
