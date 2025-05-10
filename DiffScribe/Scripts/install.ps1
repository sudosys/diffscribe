$source = Split-Path -Parent $MyInvocation.MyCommand.Path
$dest = "$env:ProgramFiles\DiffScribe"

New-Item -Path $dest -ItemType Directory -Force > $null
Copy-Item -Recurse -Force "$source\*" $dest

$currPath = [Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine)
if (-not ($currPath -split ";" | Where-Object { $_ -eq $dest })) {
    [Environment]::SetEnvironmentVariable("Path", "$currPath;$dest", [System.EnvironmentVariableTarget]::Machine)
    Write-Output "Added $dest to PATH. Restart your session."
} else {
    Write-Output "$dest is already in the PATH."
}

Read-Host "Press Enter to exit"