using System.Diagnostics;
using DiffScribe.Configuration;
using DiffScribe.Encryption;

namespace DiffScribe.Uninstall;

public abstract class AppUninstaller(ConfigHandler configHandler, SecretKeyHandler secretKeyHandler)
{
    protected abstract string UninstallScriptPath { get; }

    public virtual void Uninstall()
    {
        configHandler.RemoveConfiguration();
        secretKeyHandler.DeleteKey();
    }
    
    protected abstract ProcessStartInfo GetProcessStartInfo();
}