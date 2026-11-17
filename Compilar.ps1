param([string]$NombreSalida = 'Bodoni.Red.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$compilerPath = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compilerPath)) {
    $compilerPath = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compilerPath)) { throw 'No se encontró el compilador de .NET Framework en Windows.' }
$sourceFiles = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })
$outputPath = Join-Path $projectRoot $NombreSalida
$templatePath = Join-Path $projectRoot 'modelo nota e venta.jpg'
if (-not (Test-Path -LiteralPath $templatePath)) { throw 'No se encontró el archivo original modelo nota e venta.jpg.' }
& $compilerPath /nologo /target:winexe /platform:anycpu /optimize+ /utf8output "/out:$outputPath" "/resource:$templatePath,Bodoni.Plantilla.jpg" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar el prototipo.' }
Write-Output "Prototipo compilado: $outputPath"
