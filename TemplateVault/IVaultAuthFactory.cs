using System.Collections.Generic;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault
{
    public interface IVaultAuthFactory
    {
        IEnumerable<(string name, string description)> GetSupportedAuthTypes();
        IAuthMethodInfo GetAuth(string authType, string? mountPoint);
    }
}