$ErrorActionPreference = 'Stop'
$packagePath = Join-Path $PSScriptRoot 'entregables\Bodoni-Red'
New-Item -ItemType Directory -Path $packagePath -Force | Out-Null
$names = @('Bodoni.Red.exe', 'GUIA_RED.md', 'Habilitar_Red.ps1', 'Habilitar_Red.cmd')
$packageFiles = @()
foreach ($name in $names) {
    $source = Join-Path $PSScriptRoot $name
    if (-not (Test-Path -LiteralPath $source)) { throw "Falta el archivo $name" }
    $target = Join-Path $packagePath $name
    Copy-Item -LiteralPath $source -Destination $target -Force
    $packageFiles += $target
}
$readmePath = Join-Path $packagePath 'ABRIR_PRIMERO.txt'
@'
BODONI - NUMERACION COMPARTIDA

1. Extrae todo el ZIP a una carpeta fija local antes de abrir el programa.
2. Lee GUIA_RED.md. Elige UN SOLO computador como principal.
3. En el principal: Bodoni.Red.exe > Red y numeracion > Este computador es el principal.
4. Ultimo numero confirmado: 4121; siguiente previsto: 004122.
5. Exporta la conexion del principal e importala en los secundarios.

El principal debe estar encendido y con Bodoni abierto mientras se emita.
Los PDF de borradores muestran PRUEBA y no consumen numeros.
Emitir y asignar numero guarda una venta REAL en el principal.

Para habilitar la entrada de red en Windows, ejecuta Habilitar_Red.cmd
como administrador SOLAMENTE EN EL PRINCIPAL y con la red privada.

Este paquete no contiene datos de uso, claves ni un contador precargado
en cada equipo. La instalacion fisica en los computadores esta pendiente.
Conserva la carpeta datos_bodoni que creara el programa.
'@ | Set-Content -LiteralPath $readmePath -Encoding UTF8
$packageFiles += $readmePath
$archivePath = Join-Path $PSScriptRoot 'entregables\Bodoni-Red.zip'
Compress-Archive -LiteralPath $packageFiles -DestinationPath $archivePath -Force
Write-Output "Paquete preparado: $archivePath"
