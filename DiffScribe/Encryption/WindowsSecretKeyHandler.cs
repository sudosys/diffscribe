using System.Security.AccessControl;
using System.Security.Principal;

namespace DiffScribe.Encryption;

#pragma warning disable CA1416

public class WindowsSecretKeyHandler : SecretKeyHandler
{
    protected override void CreateRestrictedFile()
    {
        var fileInfo = new FileInfo(SecretKeyFilePath);
        
        fileInfo.Create().Close();
        fileInfo.SetAccessControl(SetFileAccessRules());
    }
    
    private FileSecurity SetFileAccessRules()
    {
        var fileSecurity = new FileSecurity();
        fileSecurity.SetAccessRuleProtection(true, false);
        
        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        fileSecurity.AddAccessRule(new FileSystemAccessRule(
            everyone,
            FileSystemRights.FullControl,
            AccessControlType.Deny));
        
        var currentUser = WindowsIdentity.GetCurrent().User;
        if (currentUser != null)
        {
            fileSecurity.AddAccessRule(new FileSystemAccessRule(
                currentUser,
                FileSystemRights.Read | FileSystemRights.Write,
                AccessControlType.Allow));
        }

        return fileSecurity;
    }

}