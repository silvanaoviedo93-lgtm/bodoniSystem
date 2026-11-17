$ErrorActionPreference = 'Stop'
$programPath = Join-Path $PSScriptRoot 'Bodoni.Red.exe'
if (-not (Test-Path -LiteralPath $programPath)) { throw 'Primero compila Bodoni.Red.exe.' }
$testProcess = Start-Process -FilePath $programPath -ArgumentList '--network-test' -WindowStyle Hidden -Wait -PassThru
Get-Content -LiteralPath (Join-Path $PSScriptRoot 'entregables\VERIFICACION_RED.txt') -Encoding UTF8
if ($testProcess.ExitCode -ne 0) { throw 'Fallaron las comprobaciones de red.' }
