using System.Diagnostics;

namespace DiffScribe.Uninstall;

public interface IAppUninstaller
{
    string UninstallScriptPath { get; }
    
    void Uninstall();
    
    ProcessStartInfo GetProcessStartInfo();
}