using System;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Vault
{
    public class VaultClientFactory : IVaultClientFactory
    {
        public IVaultClient GetVaultClient(IAuthMethodInfo auth, Uri vaultRoot)
        {
            // create the vault client object
            var vaultRootNoPath = new Uri(vaultRoot, "/");
            var vaultSettings = new VaultClientSettings(vaultRootNoPath.ToString(), auth);
            return new VaultClient(vaultSettings);
        }
    }
}