using VaultSharp.V1.AuthMethods;

namespace TemplateVault
{
    public interface IVaultAuthFactory
    {
        string[] GetSupportedAuthTypes();
        IAuthMethodInfo GetAuth(string authType, string? mountPoint);
    }
}