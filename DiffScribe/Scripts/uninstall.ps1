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

$target  = [EnvironmentVariableTarget]::Machine
$oldPath = [Environment]::GetEnvironmentVariable('Path', $target)

if ([string]::IsNullOrWhiteSpace($oldPath)) {
    Write-Output 'System PATH is empty or not found; nothing changed.'
    return
}

$paths   = $oldPath -split ';' | Where-Object { $_ -and $_ -ne $dest }
$newPath = $paths -join ';'

if ($newPath -eq $oldPath) {
    Write-Output "$dest not found in PATH."
    return
}

if (-not $newPath) {
    $newPath = 'C:\Windows\System32'
    Write-Output 'PATH would be empty; replaced with minimal safe PATH.'
}

[Environment]::SetEnvironmentVariable('Path', $newPath, $target)
Write-Output "Removed $dest from PATH. Restart your session."

Read-Host "Press Enter to exit"