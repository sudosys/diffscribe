namespace DiffScribe.Encryption;

#pragma warning disable CA1416

public class UnixSecretKeyHandler : SecretKeyHandler
{
    protected override void CreateRestrictedFile()
    {
        File.Create(SecretKeyFilePath).Close();
        File.SetUnixFileMode(SecretKeyFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }
}