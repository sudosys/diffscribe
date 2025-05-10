$source = Split-Path -Parent $MyInvocation.MyCommand.Path
$dest = "$env:ProgramFiles\DiffScribe"

New-Item -Path $dest -ItemType Directory -Force > $null
Copy-Item -Recurse -Force "$source\*" $dest

Write-Output "Application files are copied to $dest."

$currPath = [Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::Machine)
if (-not ($currPath -split ";" | Where-Object { $_ -eq $dest })) {
    [Environment]::SetEnvironmentVariable("Path", "$currPath;$dest", [System.EnvironmentVariableTarget]::Machine)
    Write-Output "$dest is added to PATH. Restart your session."
} else {
    Write-Output "$dest is already in the PATH."
}

Read-Host "Installation completed. Press ENTER to exit"