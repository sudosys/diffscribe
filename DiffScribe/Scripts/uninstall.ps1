$dest = "$env:ProgramFiles\DiffScribe"
$exe = Join-Path $dest "dsc.exe"

$proc = Get-Process | Where-Object { $_.Path -eq $exe }
if ($proc) {
    Write-Output "Waiting for dsc.exe to exit..."
    $proc.WaitForExit()
}

if (Test-Path $dest) {
    Remove-Item -Recurse -Force $dest
    Write-Output "Removed $dest and its contents."
} else {
    Write-Output "$dest does not exist."
}

$oldPath = [Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine)
$newPath = ($oldPath -split ";") | Where-Object { $_ -ne $dest } -join ";"
if ($oldPath -ne $newPath) {
    [Environment]::SetEnvironmentVariable("Path", $newPath, [System.EnvironmentVariableTarget]::Machine)
    Write-Output "Removed $dest from PATH. Restart your session."
} else {
    Write-Output "$dest not found in PATH."
}