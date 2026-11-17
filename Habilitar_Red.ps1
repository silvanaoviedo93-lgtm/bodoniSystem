param([ValidateRange(1024, 65535)][int]$Puerto = 47831)
$ErrorActionPreference = 'Stop'
$principalIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principalWindows = New-Object Security.Principal.WindowsPrincipal($principalIdentity)
if (-not $principalWindows.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Ejecuta Habilitar_Red.cmd con clic derecho > Ejecutar como administrador, solamente en el equipo principal.'
}
$programPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'Bodoni.Red.exe'))
if (-not (Test-Path -LiteralPath $programPath)) { throw 'Bodoni.Red.exe debe estar en la misma carpeta que este archivo.' }
$hashAlgorithm = [Security.Cryptography.SHA256]::Create()
try {
    $pathHash = [BitConverter]::ToString($hashAlgorithm.ComputeHash([Text.Encoding]::UTF8.GetBytes($programPath.ToLowerInvariant()))).Replace('-', '').Substring(0, 16)
} finally { $hashAlgorithm.Dispose() }
$ruleName = "Bodoni.Red.$pathHash.$Puerto"
$existingRule = Get-NetFirewallRule -Name $ruleName -ErrorAction SilentlyContinue
if ($existingRule) {
    Enable-NetFirewallRule -Name $ruleName | Out-Null
} else {
    New-NetFirewallRule -Name $ruleName -DisplayName "Bodoni Red TCP $Puerto" -Direction Inbound -Action Allow -Program $programPath -Protocol TCP -LocalPort $Puerto -Profile Private -RemoteAddress LocalSubnet | Out-Null
}
Write-Output "Conexion habilitada para Bodoni en el puerto TCP $Puerto, red privada y subred local."
Write-Output 'La regla se aplica a esta ubicacion de Bodoni.Red.exe. Si mueves el programa, ejecuta de nuevo este archivo en su nueva carpeta.'
