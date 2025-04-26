$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$parentDir = Split-Path -Parent $scriptDir

if (Test-Path (Join-Path $scriptDir "dsc.exe")) {
    $source = $scriptDir
} elseif (Test-Path (Join-Path $parentDir "dsc.exe")) {
    $source = $parentDir
} else {
    Write-Error "Could not find dsc.exe in the current or parent directory."
    exit 1
}

$dest = "$env:ProgramFiles\DiffScribe"

Copy-Item -Recurse -Force $source $dest

$currPath = [Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine)
if (-not ($currPath -split ";" | Where-Object { $_ -eq $dest })) {
    [Environment]::SetEnvironmentVariable("Path", "$currPath;$dest", [System.EnvironmentVariableTarget]::Machine)
    Write-Output "Added $dest to PATH. Restart your session."
} else {
    Write-Output "$dest is already in the PATH."
}