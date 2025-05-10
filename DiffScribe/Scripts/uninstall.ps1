$dest = "$env:ProgramFiles\DiffScribe"
$exe = Join-Path $dest "dsc.exe"

$proc = Get-Process | Where-Object { $_.Path -eq $exe }
if ($proc) {
    Write-Output "Waiting for dsc.exe to exit..."
    $proc.WaitForExit()
}

$target  = [EnvironmentVariableTarget]::Machine
$oldPath = [Environment]::GetEnvironmentVariable('Path', $target)

$paths   = $oldPath -split ';' | Where-Object { $_ -and $_ -ne $dest }
$newPath = $paths -join ';'

if ($newPath -eq $oldPath) {
    Write-Output "$dest not found in PATH."
} else {
    [Environment]::SetEnvironmentVariable('Path', $newPath, $target)
    Write-Output "Removed $dest from PATH. Restart your session."
}

Remove-Item -Recurse -Force $dest
Write-Output "Application files are removed from $dest."

Read-Host "Uninstallation completed. Press ENTER to exit"