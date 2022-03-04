using System;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Vault
{
    public interface IVaultClientFactory
    {
        IVaultClient GetVaultClient(IAuthMethodInfo auth, Uri vaultRoot);
    }
}