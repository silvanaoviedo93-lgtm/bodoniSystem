# Bodoni — Contexto y traspaso del proyecto para otra IA

Fecha de revisión: **16 de noviembre de 2026**. Carpeta de trabajo: `C:\Users\Bodoni UNO\Documents\Cotizador Bodoni`.

Este documento reúne el objetivo, las decisiones del usuario, las etapas, la implementación y los pendientes para continuar el proyecto sin depender del historial de conversación. Se preparó revisando la documentación, los ocho archivos fuente de `src/`, los scripts y los registros de verificación existentes. En esta tarea de documentación no se recompiló, no se ejecutaron de nuevo las pruebas, no se configuró un principal y no se modificaron datos de uso.

## Actualización vigente — 20/11/2026

Control posterior del rango autorizado: el usuario confirmó **4026–4225** en el pie. Se extrae del texto `# inicio-fin` al registrar el pie y se muestra en pantalla. La alerta comienza con próximo **004216** (quedan 10, inclusive el próximo); con próximo 004215 quedan 11. La barra y Última numeración muestran rango y disponibilidad. Al emitir la última 004225, se bloquea hasta registrar un rango compatible. Validación central bajo el mismo bloqueo de emisión: dos clientes no pueden superar el límite. Reintentos y recuperaciones de notas emitidas siguen disponibles; anular la última habilita solo su reemplazo dentro del rango. Renovar no reinicia ni salta el contador. Un pie anterior sin rango se conserva, bloquea nuevas emisiones y puede corregirse desde Pie de imprenta; no se infiere otro rango. Protocolo/registro actuales **4**, migración desde 1/2/3. Verificación actual: **44 generales y 133 de red**, más revisión visual de la pantalla del rango.

Actualización posterior de autorización: botón **Pie de imprenta**, datos iniciales de las imágenes del usuario: AUT. S.R.I. **1133313444**, Emisión **09/03/2026**, Caduca **09/03/2027**. La Aut. **13485** corresponde a la imprenta. Se controla en el principal con fecha de Ecuador continental (UTC-5), aviso desde 14 días antes, validez inclusive el día de caducidad y bloqueo posterior aun si se intenta usar una fecha anterior. La fecha de la nota debe estar dentro de la vigencia. Se consulta aproximadamente cada cuatro segundos, con estado visible y alerta al abrir o cambiar de día mientras el programa está abierto. No se ejecuta un servicio de avisos con Bodoni cerrado.

El pie se renueva desde el principal; campos de número, fechas y texto completo. Se leen las fechas de Emisión y Caduca del texto y se rechaza cualquier discrepancia con los campos. Se conserva historial de renovaciones y copia de la autorización usada en cada nueva nota. No cambia el contador ni los documentos anteriores. Cambiar estos datos no cambia la imagen original: cargar la imagen del formulario renovado con Cambiar plantilla en cada equipo. El PDF solo datos no imprime el pie ni la autorización preimpresa.

Ahora se usa **protocolo TCP 4 y registro 4**, con migración durable desde registros 1/2 sin cambiar números. Un registro 3 sin autorización se bloquea; no se reinicia ni se vuelve a la inicial. Las recuperaciones de solicitudes ya emitidas se resuelven antes del control de vencimiento. Solo un rechazo definitivo autenticado de autorización devuelve la solicitud no emitida a borrador; una desconexión mantiene el pendiente. Hay 44 comprobaciones generales y 116 de red correctas en la revisión actual, incluidas fechas límite, renovación, persistencia, interfaz y reintentos de bloqueos breves de Windows sin sustituir el reemplazo atómico.

- El usuario está fuera de la oficina y dispone de un solo computador. El principal previsto tiene Windows 11; aún no se ha probado la red entre equipos físicos.
- El número 4121 es solo base de práctica. El último real de producción es desconocido: confirmarlo e ingresarlo al inicializar el principal de producción.
- `Abrir_Practica.cmd` ejecuta `Bodoni.Red.exe --practica`: datos aislados en `datos_practica`, principal local por loopback, puerto 47832 y base 4121. No importa borradores ni perfiles de producción, no abre el firewall y conserva la numeración al reabrir. Vista previa y PDF identifican los números como `PRUEBA 004122`.
- La ejecución normal mantiene `datos_bodoni` y la pantalla configurable de principal/secundario, host, puerto e importación de conexión. No se inicializó producción durante este cambio.
- Nueva decisión del usuario: anular y reemplazar solo la última nota, conservando original, motivo, fecha e historial. `Anular última nota` exige motivo; `Nueva` prepara el reemplazo. No se reutiliza una nota con otra vigente posterior. Reintentar la emisión original devuelve la anulada. El número se asigna al emitir y no se reserva al consultar.
- `Última numeración` consulta el principal y muestra último número, siguiente disponible y si hay reemplazo pendiente.
- El registro admite lectura de versiones 1, 2, 3 y 4, migra y escribe versión 4. El protocolo TCP es versión 4: actualizar todos los equipos juntos; no usar ejecutables antiguos sobre el registro actualizado.
- El historial incluye anuladas y reemplazos. Exportar una emitida consulta su vigencia en el principal; las anuladas no generan PDF válido. Los PDF previamente guardados o impresos no se pueden retirar automáticamente.
- Pendientes: validación física, respaldos/restauración asistida, instalador y cotizador. Los apartados posteriores conservan el traspaso histórico; esta sección prevalece respecto a las decisiones nuevas.

## 1. Lectura rápida para retomar

El proyecto es una aplicación de escritorio para Windows destinada a Bodoni. Aunque la carpeta se llama **Cotizador Bodoni**, el módulo desarrollado primero es la **nota de venta con precios manuales**, vista previa y PDF sobre el formulario original. El cotizador todavía no está implementado.

La versión actual es **`Bodoni.Red.exe`**. Ya implementa emisión real, consecutivo central compartido, historial y recuperación de emisiones pendientes. Un computador principal guarda las notas y asigna números; los otros equipos se conectan a él mediante TCP en la red local. No se requiere internet para operar, pero sí conexión al principal para emitir.

El usuario adelantó la red antes del cotizador. El estado actual es **versión de red implementada y verificada con clientes TCP locales, pendiente de instalación y comprobación entre computadores físicos**. El usuario confirmó la impresión al tamaño correcto; esto no equivale a una aceptación documentada de la alineación de todos los campos en todas las impresoras.

Datos que no deben confundirse:

| Tema | Decisión vigente |
|---|---|
| Principal de producción | Será **otro computador**, no el de esta sesión. |
| Cantidad prevista de equipos | 2 o 3 en total; un único principal. |
| Serie visual | `001-001-000`, conservada literalmente. |
| Último número confirmado por el usuario | `4121`, presentado como `004121`. |
| Siguiente número previsto | `004122`, sujeto a revalidar el último real antes de operar. |
| Número `004026` de la referencia | Ejemplo visual de una nota anterior; no es el último consecutivo. |
| Papel y PDF | 200 × 145 mm, horizontal; imprimir a tamaño real, al 100 %. |
| Plantilla | Imagen original de Bodoni; no reemplazarla por un diseño genérico. |
| Próximo hito | Validar la instalación en la red física y el recorrido de emisión y reimpresión. |

## 2. Qué se quiere construir

El objetivo es facilitar la preparación de documentos comerciales de Bodoni, empezando por notas de venta que aprovechen su papel preimpreso. El operador ingresa cliente, fecha, cantidades, descripciones y precios; el programa calcula los importes, muestra su posición sobre el formulario y genera un PDF.

El alcance implementado incluye:

- Datos del cliente: nombre, identificación, dirección, teléfono y correo; fecha de la nota.
- Hasta diez conceptos con cantidad, descripción y precio unitario manuales.
- Cálculo decimal por línea y suma del total en dólares.
- Guardado y apertura de borradores, y recuperación automática del trámite actual.
- Vista previa con el formulario original como fondo y datos superpuestos.
- Dos salidas diferenciadas: **PDF solo datos** y **PDF con plantilla**.
- Perfiles locales para desplazamiento de impresión y tamaño de letra.
- Configuración como principal o secundario, exportación e importación de la conexión.
- Emisión con número asignado exclusivamente por el principal.
- Historial central de las últimas 200 notas para consulta y reimpresión.
- Recuperación de una emisión cuyo resultado no llegó al cliente, sin duplicarla.

El cotizador futuro permitirá crear, guardar y consultar cotizaciones con precios manuales y convertirlas en notas. La conversión deberá conservar cliente, conceptos e importes; solo la emisión de la nota consumirá numeración. Su flujo y presentación todavía deben desarrollarse.

El cálculo automático de costos de materiales, tamaños, acabados, desperdicio y margen queda para ampliaciones posteriores. Inventario, contabilidad, facturación electrónica, sincronización entre sucursales y envío de mensajes quedan fuera de la primera versión acordada.

## 3. Instrucciones y fuentes que debe leer la siguiente IA

Leer primero [AGENTS.md](AGENTS.md). Antes de diseñar o modificar la nota, leer también:

1. [PLAN_DESARROLLO.md](PLAN_DESARROLLO.md): objetivo, etapas, decisiones y criterios de aceptación.
2. [ENTREGABLE_01_ESPECIFICACION.md](ENTREGABLE_01_ESPECIFICACION.md): alcance original de la nota local y ejemplos de aceptación.
3. [REFERENCIA_PLANTILLA_NOTA_VENTA.md](REFERENCIA_PLANTILLA_NOTA_VENTA.md): distribución obligatoria, plantilla y posiciones de referencia.
4. [README.md](README.md): uso de la versión actual y comandos de desarrollo.
5. [GUIA_RED.md](GUIA_RED.md): instalación física, conexión, emisión, recuperación y conservación de datos.
6. [entregables/REVISION_RED.md](entregables/REVISION_RED.md): alcance y pendientes de la entrega actual de red.

Las instrucciones posteriores del usuario tienen prioridad. Este archivo resume el estado observado; no reemplaza las reglas del proyecto ni convierte una función planificada en implementada.

Hay documentación histórica que conserva párrafos anteriores al cambio de prioridades. Por ejemplo, la especificación local aún dice que no hace falta servidor; la revisión del prototipo aún propone implementar la emisión real; algunos comentarios y párrafos mantienen la calibración física como totalmente pendiente. Para entender el presente, contrastar esas referencias con las decisiones vigentes, la revisión de red y el código. No volver a una arquitectura exclusivamente local por leer un párrafo antiguo.

## 4. Etapas y estado real

El plan conserva siete etapas originales. El usuario cambió su orden y adelantó la etapa 7 antes de la 6. Los estados siguientes describen la evidencia disponible, sin suponer una aceptación que no está registrada.

| Etapa del plan | Trabajo | Estado observado y trabajo restante |
|---|---|---|
| 1. Definir la nota de venta | Especificar datos, precios, numeración, PDF y alcance. | Documentación disponible. El alcance evolucionó posteriormente para incluir la red. |
| 2. Revisar pantallas y plantilla | Captura, vista previa y PDF de alineación. | Prototipo y muestras entregados; plantilla original incorporada. Hay comprobaciones de controles y captura. |
| 3. Construir la nota de venta local | Captura, cálculo, borradores, emisión, consulta y PDF. | Funciones principales implementadas; emisión e historial están ahora centralizados en la versión de red. Los prototipos antiguos no emiten números reales. |
| 4. Validar impresión | Papel real, perfiles y posiciones. | Tamaño correcto confirmado por el usuario. No consta aquí una aceptación completa de todos los campos y perfiles en cada impresora física. |
| 5. Completar entrega local | Anulación, respaldo, restauración, instalador y guía. | Parcial: hay guía, ejecutable portable y procedimiento de copia manual. Faltan anulación, restauración asistida e instalador. |
| 7. Ampliar a red Wi-Fi, adelantada | Un principal, 2 o 3 equipos y consecutivo compartido. | Implementada y probada con TCP local; paquete preparado. Falta instalación y validación entre los computadores físicos. |
| 6. Incorporar el cotizador | Cotizaciones y conversión a nota. | Pendiente. Retomar después de validar la conexión de los equipos físicos. |

No hay base suficiente para asignar un porcentaje global de avance o declarar cerrado todo el proyecto. La fase activa es la **puesta en marcha y revisión de la versión de red**.

## 5. Reglas que deben conservarse

### 5.1. Formulario, vista previa y PDF

- La referencia obligatoria es [modelo nota e venta.jpg](modelo%20nota%20e%20venta.jpg), de **2247 × 1534 píxeles**. Respetar distribución, casillas, columnas, encabezado, logo y espacios.
- El archivo original se incluye sin modificar en el ejecutable como recurso `Bodoni.Plantilla.jpg`. El generador de PDF utiliza la imagen cargada como fondo; no debe recrear el formulario mediante líneas o un diseño nuevo.
- Una plantilla personalizada en `datos_bodoni/plantilla.png` tiene preferencia al cargar. La vista previa y el PDF completo deben usar la misma imagen.
- **PDF solo datos:** únicamente datos variables, número y, si corresponde al lote de papel, serie. No repetir logo, bordes, títulos, autorización, firmas ni otros elementos preimpresos.
- **PDF con plantilla:** fondo completo y datos para revisión o papel blanco. La serie ya visible en el fondo no se superpone de nuevo.
- `PrintSeries` afecta únicamente a la salida solo datos. El valor predeterminado del código es `false`, porque el formulario original ya trae la serie.
- Las guías de campos pertenecen a la vista previa y no se exportan.
- Mantener ambos botones y nombres de archivo distintos: `Bodoni-<número>-solo-datos.pdf` y `Bodoni-<número>-completa.pdf`.
- El tamaño físico es siempre **200 × 145 mm**. Vista previa y PDF comparten las posiciones en milímetros. No usar directamente los píxeles como medidas del papel.
- El consecutivo va debajo de la serie y encima de la autorización. La referencia visual usa seis dígitos, ceros a la izquierda y color rojo.
- La Epson L3250 es la referencia inicial. Validar cada impresora con papel compatible, tamaño real y escala 100 %, sin ajuste automático.
- Si el texto excede su casilla o hay más de diez filas, avisar y bloquear la operación estricta. No cortar contenido silenciosamente ni invadir otras zonas.

### 5.2. Numeración, emisiones y recuperación

- Usar un único principal y un único registro central en su disco local. Los secundarios nunca asignan números por su cuenta.
- Guardar nota y contador juntos, de forma durable, antes de confirmar la emisión.
- Borradores, consultas, vistas previas y pruebas de alineación no consumen números. Un borrador muestra `PRUEBA`.
- La numeración se consume al emitir, no al exportar PDF ni al imprimir.
- Un doble clic o reintento de **la misma solicitud** debe devolver la misma nota. Esto depende de conservar su `RequestId`.
- Persistir la solicitud antes de enviarla. Si se pierde la respuesta, conservar identificador, contenido y principal originales, y recuperar esa solicitud.
- No sustituir una emisión pendiente, generar otro identificador para repetirla ni trasladarla a otro principal.
- Si falla el PDF o la impresión después de emitir, recuperar la nota emitida y conservar su número.
- Las notas emitidas son inmutables en la versión actual. La anulación futura deberá conservar documento y número; nunca reutilizarlo.
- Un registro central ausente o dañado en un principal inicializado bloquea la emisión. No reiniciar desde `4121` ni borrar las marcas para eludir el bloqueo.
- Revalidar el último consecutivo real antes de la primera instalación operativa. `004122` es una previsión, no una reserva.
- Las pruebas deben utilizar registros aislados y un servidor TCP local de prueba, sin tocar datos de uso. No afirmar que se probaron computadores físicos cuando solo hubo clientes locales.

## 6. Implementación técnica

### 6.1. Plataforma y estructura

Aplicación **C# con Windows Forms sobre .NET Framework**. Compila con el `csc.exe` disponible en Windows, sin paquetes NuGet ni instalación de dependencias de terceros. El repositorio observado no contiene solución `.sln` ni proyecto `.csproj`: `Compilar.ps1` reúne todos los `src/*.cs`.

El script busca el compilador en `Microsoft.NET\Framework64\v4.0.30319\csc.exe` y, como alternativa, en `Framework\v4.0.30319`. Genera un ejecutable `winexe`, `AnyCPU`, optimizado, e incrusta la plantilla original.

Referencias de compilación: `System.dll`, `System.Core.dll`, `System.Drawing.dll`, `System.Windows.Forms.dll` y `System.Web.Extensions.dll`. La compatibilidad concreta de Windows y .NET Framework en el otro equipo aún debe comprobarse. No asumir que utiliza .NET moderno, WPF, una aplicación web o una base SQL.

### 6.2. Mapa del código

| Archivo | Responsabilidad y puntos de entrada útiles |
|---|---|
| `src/Program.cs` | Inicio `[STAThread]`, bloqueo de instancia por carpeta de datos y modos `--self-test`, `--network-test`, `--ui-check`. Incluye `Verification`, la suite de regresión. |
| `src/Core.cs` | Modelos `Draft`, `DraftLine`, `Profile`, `Settings`; almacenamiento y migración; plantilla incrustada; cálculo decimal; lectura de métricas de fuentes; `Layout`; generador `Pdf`. |
| `src/MainForm.cs` | Editor del cliente y detalle, totales, borradores, recuperación de sesión, perfiles, cambio de plantilla, vista previa y exportación de las dos modalidades PDF. Incluye comprobaciones de controles. |
| `src/IssueWorkflow.cs` | `WorkState` e `IssueWorkflow`: persistencia del trámite, transiciones entre borrador, pendiente y emitido, validación de la respuesta y bloqueo de sustituciones. |
| `src/Network.cs` | Configuración, normalización y huella del documento; modelos de emisión; `CentralLedger`; protocolo TCP; `NumberServer` y `NumberClient`. |
| `src/NetworkUI.cs` | Segunda parte de `MainForm`: conexión, servicio del principal, consulta periódica, emisión y recuperación, historial y exportación de conexión. |
| `src/NetworkSetupForm.cs` | Pantallas `NetworkSetupForm` y `NoteHistoryForm`: principal/secundario, importación y consulta de últimas 200 notas. |
| `src/NetworkVerification.cs` | Suite de pruebas de red con datos aislados, concurrencia, reintentos, desconexión y fallos de persistencia. |

### 6.3. Modelos y cálculos

`Draft` tiene versión 1, cliente, identificación, dirección, teléfono, correo, fecha `yyyy-MM-dd` y lista de `DraftLine`. Cantidad y precio se conservan como texto en el borrador, lo que permite guardarlo incompleto; se validan al calcular o emitir.

`Calculation` acepta coma o punto decimal, sin separadores de miles. Una fila totalmente vacía se omite; una parcialmente llena debe ser válida. Reglas actuales:

- Cantidad mayor que cero, hasta 1.000.000 y con hasta tres decimales.
- Precio entre cero y 1.000.000.000, con hasta cuatro decimales. El precio cero está permitido.
- Descripción obligatoria en cada concepto y al menos un concepto; máximo diez.
- Importe = cantidad × precio, redondeado a dos decimales con `MidpointRounding.AwayFromZero`.
- Total = suma de los importes ya redondeados. No usar `double` para estos cálculos monetarios.
- Precio impreso con formato `0.00##`, cantidad `0.###` e importes `0.00`.

`NoteRules.Normalize` exige cliente y fecha válidos, normaliza números y textos y produce un documento estable. `Fingerprint` es SHA-256 de su JSON normalizado. `IssuedNote` agrega `RequestId`, `ServerId`, serie, número, momento de emisión UTC, nombre del equipo, huella y total. El número se presenta con `D6`.

### 6.4. Posiciones, fuentes y PDF

`Layout.Build` es la fuente compartida de posiciones para vista previa y PDF. Crea una lista de `TextItem`; los ajustes de perfil se aplican a esas posiciones. Las coordenadas principales, antes del desplazamiento del perfil, son:

| Campo | X (mm) | Línea base desde arriba (mm) | Ancho (mm) |
|---|---:|---:|---:|
| Serie opcional | 156,5 | 9,0 | 42,2 |
| Consecutivo | 156,5 | 15,7 | 42,2 |
| Cliente | 13,8 | 37,2 | 125 |
| Fecha | 164,1 | 37,2 | 34,5 |
| Dirección | 16,8 | 44,3 | 131 |
| Teléfono | 164,1 | 44,3 | 34,5 |
| Identificación | 20 | 51,4 | 91 |
| Correo | 122,5 | 51,4 | 76,1 |
| Cantidad / descripción / precio / importe | 1,4 / 15 / 148,7 / 172,2 | Primera fila 64,7; intervalo 6,18 | 10,8 / 131,5 / 21 / 26 |
| Total | 172,2 | 131,0 | 26 |

`Baseline` es la línea base del texto, no su borde superior. La conversión a puntos PDF es `72 / 25.4`. `Layout.Build(..., strict: true)` comprueba contenido, anchos y límites de página. La vista previa puede usar la construcción no estricta y mostrar el aviso de validación.

Los perfiles guardan nombre, `OffsetX`, `OffsetY`, `FontScale` y `PrintSeries`. Desplazamientos admitidos: ±10 mm; escala de letra: 80 % a 120 %. Los valores positivos mueven a la derecha y hacia abajo. No hay todavía un editor visual de coordenadas individuales; esas posiciones están definidas en `Layout.Build`.

El PDF se construye directamente en C#, con objetos PDF 1.4, referencias, texto y fuentes TrueType incrustadas. Usa Arial para el cuerpo y Times New Roman para el número. Lee `arial.ttf` y `times.ttf` de la carpeta de fuentes de Windows y utiliza sus métricas para medir el texto. El contenido admitido corresponde a Windows-1252/WinAnsi: los caracteres no representables y los saltos de línea se rechazan para evitar salidas incorrectas. No hay ajuste automático de descripciones a varias líneas.

El PDF completo incrusta como imagen el fondo cargado, a sus dimensiones de píxeles completas, y dibuja los datos encima. El archivo JPG original se conserva intacto como recurso; el proceso de exportación codifica la imagen cargada para el PDF. `Pdf.SaveIssued` utiliza el contenido y número de `IssuedNote`, no el próximo número consultado al servidor.

### 6.5. Arquitectura de red

El mismo ejecutable puede actuar como principal o secundario. El principal inicia `NumberServer` dentro del proceso de Windows Forms, escucha con `TcpListener` y conserva el registro local. **No es un servicio de Windows**: el equipo debe permanecer encendido, sin suspensión, y Bodoni abierto.

Los secundarios usan `NumberClient` y `TcpClient`. El principal también emite a través de su cliente por `127.0.0.1`. El puerto predeterminado es **TCP 47831**. La interfaz consulta el estado aproximadamente cada cuatro segundos, ejecutando operaciones de red fuera del hilo de interfaz.

El protocolo utiliza JSON UTF-8 precedido por una longitud de cuatro bytes en orden de red; el límite por mensaje es 2 MiB. Las operaciones actuales son `status`, `issue`, `find` y `recent`. Cada solicitud incluye versión, clave de conexión e identificación `ServerId`; la respuesta debe corresponder al mismo principal. La configuración del principal genera una clave aleatoria de 32 bytes y un GUID para `ServerId`.

La autenticación actual es mediante clave compartida. El transporte es TCP directo, sin TLS; el alcance acordado es la red local de la empresa. No hay cuentas individuales de operador ni roles de usuario. El archivo de conexión contiene credenciales y no debe incluirse en paquetes públicos ni en este documento.

`CentralLedger` serializa las operaciones con `lock`, bloquea la apertura simultánea del mismo registro mediante un archivo abierto con `FileShare.None` y rechaza rutas UNC de carpeta compartida. El historial conserva todas las notas en el registro; la consulta `Recent()` devuelve como máximo las 200 últimas. No hay búsqueda avanzada ni paginación de las anteriores en la interfaz actual.

### 6.6. Guardado durable e idempotencia

El registro no usa SQLite ni otro motor de base de datos. `CentralState` contiene versión, identificación del servidor, serie, número inicial, último número y todas las notas. Se serializa dentro de `LedgerEnvelope`, con `Payload` y su checksum SHA-256.

Al emitir, el principal valida el documento y su `RequestId`. Si ese identificador ya existe con la misma huella, devuelve la nota existente. Si el contenido es distinto, rechaza el conflicto. Si es nuevo, prepara un estado candidato con nota y contador incrementado y lo guarda conjuntamente.

`Storage.WriteBytes` escribe un temporal en el mismo directorio, fuerza `FileStream.Flush(true)` y hace `File.Replace` o `File.Move`. El estado en memoria solo se sustituye después de guardar correctamente. Un fallo no debe adelantar el contador en memoria ni confirmar una nota parcial.

Al abrir el registro se comprueban checksum, servidor, serie, secuencia, identificadores únicos, huellas y totales. La marca `.inicializado` y `NetworkConfig.Initialized` impiden crear un registro vacío cuando falta el de un principal ya inicializado. Si llega a `999999`, la emisión se bloquea: el formato no se reinicia automáticamente.

Cada emisión reescribe el registro completo, y la apertura valida sus notas. Es una limitación de crecimiento a considerar si aumenta el volumen; cualquier migración futura debe preservar emisión conjunta, secuencia e idempotencia.

## 7. Estados del trámite y flujo operativo

`IssueWorkflow` guarda `WorkState` en `datos_bodoni/tramite-actual.json`. Los estados relevantes son:

| Estado | Representación | Conducta |
|---|---|---|
| Borrador | `Pending = false`, sin `Issued` | Editable; PDF marcado `PRUEBA`; no consume número. |
| Emisión pendiente | `Pending = true`, con `RequestId` y `ServerId` persistidos | Edición y sustitución bloqueadas; recuperar contra el mismo principal. Se bloquea exportar PDF hasta conocer el resultado. |
| Emitido o consultado | `Issued` presente y `Pending = false` | Contenido bloqueado; PDF y reimpresión con el mismo número. `Nueva` permite iniciar otra venta. |

Recorrido de una venta:

1. Crear o abrir borrador, completar cliente y conceptos y revisar la distribución.
2. Validar y confirmar **Emitir y asignar número**.
3. `Prepare` normaliza el documento y persiste un GUID de solicitud y el principal antes de enviarlo.
4. El principal guarda nota y consecutivo y devuelve `IssuedNote`.
5. `Complete` valida que la respuesta corresponde a la solicitud y guarda el estado emitido.
6. Generar el PDF apropiado; si falla su guardado o la impresión, repetir usando esa misma nota.

**Recuperar emisión** reenvía la misma solicitud: si el principal ya la guardó devuelve su número; si no, completa esa solicitud con el siguiente disponible. Cerrar el cliente no autoriza a crear otra solicitud para reemplazarla.

`MainForm` recupera primero la sesión anterior y luego el trámite actual, que tiene prioridad para preservar pendientes y notas consultadas. El autoguardado del borrador usa un temporizador de 600 ms y también se ejecuta al cerrar. No tratar `ultima-sesion.bodoni.json` como sustituto del trámite pendiente.

## 8. Archivos de datos y migración

Las rutas de datos son relativas a la carpeta del ejecutable. Abrir una copia desde otra carpeta puede crear otro conjunto de datos: usar una ubicación local fija y conservar su carpeta asociada.

| Ruta | Contenido o finalidad |
|---|---|
| `datos_bodoni/conexion-red.json` | Rol, host, puerto, clave, servidor y configuración inicial. Se crea al configurar la red. |
| `datos_bodoni/tramite-actual.json` | Borrador actual, solicitud pendiente o nota emitida/consultada; fuente de recuperación del flujo. |
| `datos_bodoni/ultima-sesion.bodoni.json` | Borrador/sesión compatible con la recuperación anterior. |
| `datos_bodoni/perfiles.json` | Perfiles locales de impresión y selección actual. |
| `datos_bodoni/plantilla.png` | Plantilla personalizada local, si existe. |
| `datos_bodoni/cliente-abierto.lock` | Bloqueo de instancia por carpeta de datos. La existencia del archivo por sí sola no prueba que haya un proceso activo. |
| `datos_bodoni/principal/registro-central.json` | Registro de notas y contador del principal. |
| `datos_bodoni/principal/registro-central.json.inicializado` | Marca que impide reiniciar una secuencia si desaparece el registro. |
| `datos_bodoni/principal/registro-central.json.lock` | Bloqueo exclusivo del registro mientras está abierto. |
| `datos_prototipo/` | Datos anteriores que se conservan; no borrar sus originales. |
| Archivos `*.bodoni.json` elegidos por el usuario | Borradores guardados con nombre propio; pueden estar fuera de la carpeta de datos. |

`Storage.ImportLegacyPrintingAndDraft` copia `perfiles.json`, `plantilla.png` y `ultima-sesion.bodoni.json` desde `datos_prototipo` solo si falta cada destino. Si no existe el trámite nuevo, se inicia a partir del borrador recuperado. `Settings.UpgradeTemplate` realiza una migración de perfiles antiguos para desactivar la duplicación de serie una sola vez; no debe borrar elecciones posteriores del usuario.

La copia manual indicada consiste en cerrar Bodoni en el principal, sin emisiones en curso, y copiar **toda su carpeta `datos_bodoni`**. Para preservar todo el entorno hay que considerar además borradores con nombre propio, PDF externos y ajustes locales de los secundarios. No hay restauración asistida. Restaurar un registro antiguo exige reconciliar las notas emitidas después del respaldo antes de continuar la secuencia.

### Estado observado en esta carpeta al preparar el traspaso

- Existen `datos_bodoni/` y `datos_prototipo/`, además de los ejecutables y entregables.
- No existe aquí `datos_bodoni/conexion-red.json` ni `datos_bodoni/principal/registro-central.json`. Esta observación local no informa sobre el estado de otros computadores.
- El trámite local es versión 1, no está pendiente y no contiene una nota emitida ni identificadores de solicitud/servidor.
- El perfil guardado es `Epson L3250`, con desplazamientos X/Y en 0 y escala 1 (100 %).
- **Ese perfil local tiene `PrintSeries = true`**, aunque el valor predeterminado del código es `false`. No se cambió al preparar este archivo. Antes de imprimir, revisar si el lote de papel ya trae la serie; conservar las decisiones explícitas del usuario y evitar duplicarla. No confundir el valor guardado con el valor inicial del programa.

Estos datos son una fotografía del 16/11/2026 y deben volver a comprobarse si la siguiente sesión encuentra archivos diferentes. No se incluyen datos de clientes ni claves.

## 9. Ejecutables, paquete y herramientas

| Archivo o carpeta | Uso |
|---|---|
| `Bodoni.Red.exe` | Versión actual para continuar el desarrollo y la instalación. |
| `Bodoni.Prototipo.exe`, `Bodoni.Prototipo.Actualizado.exe` | Versiones históricas de prueba; no usar para emisión compartida real. |
| `Compilar.ps1` | Compila todos los fuentes e incrusta la imagen; salida predeterminada `Bodoni.Red.exe`. |
| `Verificar.ps1` | Ejecuta `--self-test`, muestra el registro y falla si el proceso devuelve error. |
| `Verificar_Red.ps1` | Ejecuta `--network-test` con datos aislados y muestra el resultado. |
| `Preparar_Paquete_Red.ps1` | Copia ejecutable y guía/scripts a `entregables/Bodoni-Red/` y genera el ZIP. No recompila por sí mismo. |
| `Habilitar_Red.ps1`, `Habilitar_Red.cmd` | Regla de firewall para Bodoni en el principal, con ejecución como administrador. |
| `entregables/Bodoni-Red.zip` | Paquete preparado para trasladar a los equipos. No incluye datos de uso, claves ni contador precargado. |
| `entregables/Bodoni-Red/` | Archivos del paquete y `ABRIR_PRIMERO.txt`. |

El firewall permite entrada para la ruta concreta de `Bodoni.Red.exe`, puerto elegido, perfil privado y subred local. Si se mueve el ejecutable de producción, revisar la regla en el principal. No ejecutar ese script aquí para simular que el otro computador ya quedó configurado.

El ejecutable y ZIP existentes tienen fecha de modificación del 12/11/2026. Al cambiar código, recompilar y volver a preparar el paquete para evitar distribuir una versión anterior. Los nombres y algunos mensajes internos todavía usan «prototipo» por historia del proyecto; no implican que el código actual carezca de emisión real.

## 10. Cómo compilar y verificar

Desde PowerShell, situado en la raíz del proyecto:

```powershell
# Compilar la versión actual.
powershell -NoProfile -ExecutionPolicy Bypass -File .\Compilar.ps1

# Verificar cálculos, almacenamiento, plantilla, PDF y controles.
powershell -NoProfile -ExecutionPolicy Bypass -File .\Verificar.ps1

# Verificar emisión central, concurrencia y recuperación con TCP local.
powershell -NoProfile -ExecutionPolicy Bypass -File .\Verificar_Red.ps1

# Actualizar el paquete después de compilar y verificar cambios.
powershell -NoProfile -ExecutionPolicy Bypass -File .\Preparar_Paquete_Red.ps1
```

`ExecutionPolicy Bypass` se aplica al proceso indicado, no cambia permanentemente la política del equipo. La carpeta debe permitir la escritura de ejecutable, datos y entregables. Si el ejecutable está abierto y bloquea la compilación, coordinar su cierre preservando cualquier emisión pendiente.

Las pruebas están integradas en el propio ejecutable; no hay un proyecto de pruebas separado. Recompilar antes de probar cambios para que los scripts no verifiquen un binario anterior. `--ui-check` abre temporalmente una ventana y genera captura y registro; reservarlo para revisión de interfaz con el acceso de escritorio correspondiente.

Las suites usan directorios temporales aislados y escriben reportes y muestras en `entregables/`. Las pruebas de red utilizan loopback y puerto asignado para la prueba; no requieren abrir el firewall ni configurar el principal de uso.

### Evidencia ya existente

| Evidencia | Resultado registrado y alcance |
|---|---|
| `entregables/VERIFICACION.txt` | **44 comprobaciones correctas**: decimales, redondeo, límites, plantilla, PDF, persistencia y controles. |
| `entregables/VERIFICACION_RED.txt` | **63 comprobaciones correctas**: tres clientes TCP, secuencia, reintentos, pendientes, reinicio, fallos y bloqueo ante pérdida/corrupción del registro. |
| `entregables/REVISION_PANTALLA.txt` | Registro de apertura visible, captura, edición del detalle y validación de cliente. |
| `entregables/REVISION_RED.md` | Revisión de la entrega de red y lista de instalación pendiente. Registra lectura correcta de PDF por Ghostscript y revisión de sintaxis de scripts. |
| `entregables/REVISION_ENTREGA_01.md` | Revisión histórica del prototipo; sus pendientes no son todos vigentes. |

Las 30 emisiones concurrentes de prueba produjeron `004122` a `004151` en un registro aislado; otros casos de la suite probaron reintentos y recuperación con números posteriores. **No son ventas reales ni cambian el último número confirmado del negocio**. Las pruebas cubren que un mismo identificador no duplica notas, que la desconexión no habilita números locales, que fallar al guardar no adelanta el contador y que un registro faltante o corrupto no se reinicia.

Muestras útiles: `Bodoni-PRUEBA-completa.pdf`, `Bodoni-PRUEBA-completa-diez-filas.pdf`, `Bodoni-PRUEBA-alineacion.pdf`, `Bodoni-PRUEBA-diez-filas.pdf`, `ejemplo-50-dolares.bodoni.json` y `SIMULACION-RED-nota-numerada.pdf`, dentro de `entregables/`. El ejemplo base es **100 × $0,25 + 2 × $12,50 = $50,00**. Las muestras históricas usan fecha de prueba 12/11/2026; una nota nueva toma inicialmente la fecha local del equipo.

Los resultados anteriores se leyeron de los reportes existentes; no se ejecutaron de nuevo para redactar este documento. La lectura de PDF y las pruebas de software no sustituyen la impresión física ni la validación de la red entre equipos reales.

## 11. Próximos pasos para continuar

### 11.1. Prioridad inmediata: validar la red física

1. Revisar este contexto y las fuentes obligatorias, sin modificar los datos de uso para hacer pruebas.
2. Confirmar con el usuario cuál es el **otro computador principal**, su Windows/.NET Framework y el último consecutivo realmente utilizado en ese momento.
3. Seguir `GUIA_RED.md`: extraer el paquete en una carpeta fija local del principal, abrirlo y configurarlo una sola vez con el último número revalidado.
4. Habilitar el firewall únicamente en ese principal y con el puerto configurado, red privada y subred local.
5. Exportar la conexión e importarla en los secundarios. Si el nombre no resuelve, utilizar la IPv4 del principal. Evitar una red de invitados que aísle dispositivos.
6. Comprobar desde al menos dos computadores el mismo servidor y último número, **sin emitir ventas ficticias en el registro real**. El «Próximo» mostrado no reserva un número.
7. Con la siguiente venta real, verificar emisión, actualización del estado, consulta desde otro equipo y regeneración de ambos PDF con el mismo número.
8. Comprobar recuperación de conexión y reinicio preservando solicitudes y datos. Los ensayos que creen notas ficticias corresponden a un entorno aislado.
9. Registrar qué equipos físicos se probaron, resultado, versión del ejecutable y observaciones de impresión. Separar esta evidencia de las pruebas locales existentes.

La implementación de red ya existe: el siguiente paso no es construirla desde cero. Tampoco inicializar un principal de producción en el computador de esta sesión, porque contradice la elección del usuario.

### 11.2. Completar la entrega y preparar el cotizador

- Resolver observaciones del uso real y revisar la alineación de número, cliente, primera/última fila y total. Conservar los perfiles aceptados.
- Definir destino, frecuencia y responsable de copias; implementar y probar una restauración asistida que no retroceda la secuencia sin reconciliación.
- Implementar anulación conservando contenido y número; no añadir edición directa de emitidos como sustituto.
- Preparar instalador si se mantiene ese entregable. El ZIP portable actual no es un instalador.
- Actualizar guía, revisión y paquete con cada entrega comprobada.
- Después de validar la red física, desarrollar el módulo de cotizaciones y la conversión a nota. Definir los detalles faltantes de ese módulo antes de suponer reglas comerciales nuevas.

## 12. Límites y decisiones aún abiertas

- No consta la instalación en los computadores físicos ni su versión exacta de Windows/.NET Framework.
- Se confirmó tamaño de impresión; falta dejar evidencia suficiente de la calibración por impresora y de cualquier ajuste adicional requerido.
- El perfil local tiene una elección guardada de serie que debe revisarse frente al formulario utilizado; no cambiarla a ciegas.
- No hay anulación, restauración asistida, respaldo automático, instalador ni cotizador implementados.
- No hay modo de emisión independiente sin conexión ni múltiples principales sincronizados.
- No hay interfaz de edición de posiciones por campo, detalle de más de diez filas, descripciones multilínea ni soporte general de Unicode.
- El historial visible se limita a las últimas 200 notas; las anteriores siguen dentro del registro.
- Faltan decisiones operativas sobre destino de respaldos y recuperación. Una copia antigua nunca debe convertirse en un nuevo contador sin reconciliar lo emitido después.
- El redondeo y límites actuales están implementados y probados; cualquier cambio comercial requiere acordarlo y preservar la interpretación de notas ya emitidas.

## 13. Instrucción de continuidad para la siguiente IA

Continúa desde la versión `Bodoni.Red.exe` y el código de `src/`. Conserva el formulario original, las dos modalidades PDF, las posiciones compartidas en milímetros y las garantías de numeración y recuperación. Lee `AGENTS.md` y los documentos obligatorios antes de modificar la nota. Distingue siempre lo pedido, lo implementado, lo verificado localmente y lo aceptado en equipos físicos.

La prioridad vigente es completar la validación de la red física con **otro computador como principal** y el último número revalidado; después se retomará el cotizador. Si el usuario da una nueva prioridad, sigue esa instrucción y actualiza este contexto con lo realmente cambiado y probado. Conserva los datos y cualquier solicitud pendiente durante todo el trabajo.
