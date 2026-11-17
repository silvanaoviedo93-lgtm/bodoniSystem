$ErrorActionPreference = 'Stop'
$programPath = Join-Path $PSScriptRoot 'Bodoni.Red.exe'
if (-not (Test-Path -LiteralPath $programPath)) { throw 'Primero ejecuta Compilar.ps1.' }
$testProcess = Start-Process -FilePath $programPath -ArgumentList '--self-test' -WindowStyle Hidden -Wait -PassThru
Get-Content -LiteralPath (Join-Path $PSScriptRoot 'entregables\VERIFICACION.txt') -Encoding UTF8
if ($testProcess.ExitCode -ne 0) { throw 'Fallaron las comprobaciones del prototipo.' }
